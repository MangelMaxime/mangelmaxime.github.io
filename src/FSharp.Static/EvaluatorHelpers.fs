namespace FSharp.Static.Evaluator

open FSharp.Compiler.Diagnostics
open FSharp.Compiler.Interactive.Shell
open FSharp.Quotations.Evaluator
open FSharp.Reflection
open System.Text
open System
open System.IO
open FsToolkit.ErrorHandling
open FSharp.Static.Core
open System.Reflection
open FSharp.Static

[<RequireQualifiedAccess>]
module Typeof =

    let context = typeof<Context>
    let unit = typeof<Unit>
    let config = typeof<Config>

[<RequireQualifiedAccess>]
module EvaluatorHelpers =

    let private sbOut = StringBuilder()
    let private sbErr = StringBuilder()

    let fsi (context : Context) =
        let inStream = new StringReader("")
        let outStream = new StringWriter(sbOut)
        let errStream = new StringWriter(sbErr)
        let fsiConfig = FsiEvaluationSession.GetDefaultConfiguration()

        let argv =
            [|
                "--noframework"
                if context.IsWatch then "--define:WATCH"
                "--define:FSHARP_STATIC"
                "/temp/fsi.exe"
            |]

        try
            FsiEvaluationSession.Create(
                fsiConfig,
                argv,
                inStream,
                outStream,
                errStream
            )
        with
        | ex ->
            [
                "Error while running FSI"
                ""
                $"Exception: %A{ex}"
                ""
                $"InnerException: %A{ex.InnerException}"
                ""
                $"Error stream: %s{errStream.ToString()}"
            ]
            |> String.concat "\n"
            |> Log.error

            raise ex

    let getModuleName (path: string) =
        let fileName = Path.GetFileNameWithoutExtension path
        (string fileName[0]).ToUpperInvariant() + fileName[1..]

    let getOpenInstruction (path: string) =
        $"open %s{getModuleName path};;"

    let getLoadInstruction (path: string) =
        let sanitazedPath = path.Replace("\\", "\\\\")
        $"#load \"%s{sanitazedPath}\";;"

    module FSharpDiagnostic =

        let toText (error: FSharpDiagnostic) =
            $"{error.Start}-{error.End} {error.Message}"

    let tryLoad (fsi: FsiEvaluationSession) (path: string) =
        let _, loadErrors =
            fsi.EvalInteractionNonThrowing(getLoadInstruction path)

        if loadErrors.Length > 0 then
            let msg =
                [
                    $"Failed to load script"
                    ""
                    $"Script: '{path}'"
                    ""
                    for error in loadErrors do
                        FSharpDiagnostic.toText error
                ]
                |> String.concat "\n"

            Error msg
        else
            Ok()

    let tryOpen (fsi: FsiEvaluationSession) (path: string) =
        let _, openErrors =
            fsi.EvalInteractionNonThrowing(getOpenInstruction path)

        if openErrors.Length > 0 then
            let msg =
                [
                    $"Failed to open module '{getModuleName path}'"
                    ""
                    $"Script: '{path}'"
                    ""
                    for error in openErrors do
                        FSharpDiagnostic.toText error
                ]
                |> String.concat "\n"

            Error msg
        else
            Ok()

    let tryEvaluateCode
        (fsi: FsiEvaluationSession)
        (path: string)
        (code: string)
        =
        let evaluationResult, evalErrors = fsi.EvalExpressionNonThrowing(code)

        if evalErrors.Length > 0 then
            let msg =
                [
                    $"Failed to evaluate code"
                    ""
                    $"Script: '{path}'"
                    ""
                    for error in evalErrors do
                        FSharpDiagnostic.toText error
                ]
                |> String.concat "\n"

            Error msg
        else
            match evaluationResult with
            | Choice1Of2 (Some evaluationResult) -> Ok evaluationResult
            | Choice1Of2 None ->
                let msg =
                    [
                        $"Failed to evaluate code"
                        $"Script: '{path}'"
                    ]
                    |> String.concat "\n"

                Error msg

            | Choice2Of2 exn ->
                let msg =
                    [
                        $"Failed to evaluate code"
                        $"Script: '{path}'"
                        "Error:"
                        exn.Message
                    ]
                    |> String.concat "\n"

                Error msg


    let compileExpression (input: FsiValue) =
        let expr = input.ReflectionValue :?> Quotations.Expr
        QuotationEvaluator.CompileUntyped expr

    let private invokeFunction (func: obj) (args: obj seq) =

        // Recusive partial evaluation of f, terminate when no args are left.
        let rec helper (next: obj) (args: obj list) =
            match args with
            | head :: tail ->
                let fType = next.GetType()

                if FSharpType.IsFunction fType then
                    let methodInfo =
                        fType.GetMethods()
                        |> Array.filter (fun x ->
                            x.Name = "Invoke" && x.GetParameters().Length = 1
                        )
                        |> Array.head

                    let res =
                        methodInfo.Invoke(
                            next,
                            [|
                                head
                            |]
                        )

                    helper res tail
                else
                    None // Error case, arg exists but can't be applied
            | [] -> Some next

        helper func (args |> List.ofSeq)

    let checkParameterType
        (parameterRank: int)
        (parameter: ParameterInfo)
        (expectedType: Type)
        =
        if parameter.ParameterType = expectedType then
            Ok()
        else
            let expectedTypeName = expectedType.FullName.Replace('+', '.')

            Error
                $"Parameter #{parameterRank} has type '{parameter.ParameterType.Name}' but expected '{expectedTypeName}'"
