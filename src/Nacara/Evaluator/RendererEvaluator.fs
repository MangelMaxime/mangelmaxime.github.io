namespace Nacara.Evaluator

open FsToolkit.ErrorHandling
open Nacara.Core
open System.Reflection

[<RequireQualifiedAccess>]
module RendererEvaluator =

    let private checkSignature (func: MethodInfo) =
        let parameters = func.GetParameters()

        result {
            do! EvaluatorHelpers.checkParameterType 1 parameters[0] Typeof.context
            do! EvaluatorHelpers.checkParameterType 2 parameters[1] Typeof.pageContext

            if func.ReturnType = Typeof.string then
                return! Ok()
            else
                return! Error "Render function should return 'string'"
        }

    let tryEvaluate (render: AbsolutePath.T) (context: Context) (pageContext : PageContext) =
        use fsi = EvaluatorHelpers.fsi context
        let renderPath = AbsolutePath.toString render

        result {
            do! EvaluatorHelpers.tryLoad fsi renderPath
            do! EvaluatorHelpers.tryOpen fsi renderPath

            let! renderFunc =
                EvaluatorHelpers.tryEvaluateCode fsi renderPath "<@@ fun a b -> render a b @@>"

            let renderExpr = EvaluatorHelpers.compileExpression renderFunc
            let renderType = renderExpr.GetType()

            let renderInvokeFunc =
                renderType.GetMethods()
                |> Array.filter (fun method ->
                    method.Name = "Invoke" && method.GetParameters().Length = 2
                )
                |> Array.head

            // Valid the signature of the render function
            do! checkSignature renderInvokeFunc

            // Execute the render
            let untypedResult =
                renderInvokeFunc.Invoke(
                    renderExpr,
                    [|
                        box context
                        box pageContext
                    |]
                )

            match tryUnbox<string> untypedResult with
            | Some result ->
                return! Ok result

            | None ->
                return! Error "Render function should return 'string'"
        }
        |> Result.mapError (fun error ->
            [
                $"Invalid loader: {renderPath}"
                $"Error: {error}"
                """
Example:

open FSharp.Static.Core

let render (projectRoot: ProjectRoot.T) (context: Context) =
    // Your code goes here
    ()"""
            ]
            |> String.concat "\n"
        )
