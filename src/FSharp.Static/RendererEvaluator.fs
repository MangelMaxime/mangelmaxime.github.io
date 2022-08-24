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

[<RequireQualifiedAccess>]
module RendererEvaluator =

    let private checkSignature (func: MethodInfo) =
        let parameters = func.GetParameters()

        result {
            do! EvaluatorHelpers.checkParameterType 1 parameters[0] Typeof.context

            if func.ReturnType = Typeof.unit then
                return! Ok()
            else
                return! Error $"Loader function should return '{Typeof.unit.Name}'"
        }

    let tryEvaluate (loader: AbsolutePath.T) (context: Context) =
        use fsi = EvaluatorHelpers.fsi context
        let loaderPath = AbsolutePath.toString loader

        result {
            do! EvaluatorHelpers.tryLoad fsi loaderPath
            do! EvaluatorHelpers.tryOpen fsi loaderPath

            let! loaderFunc =
                EvaluatorHelpers.tryEvaluateCode fsi loaderPath "<@@ fun a -> loader a @@>"

            let loaderExpr = EvaluatorHelpers.compileExpression loaderFunc
            let loaderType = loaderExpr.GetType()

            let loaderInvokeFunc =
                loaderType.GetMethods()
                |> Array.filter (fun method ->
                    method.Name = "Invoke" && method.GetParameters().Length = 1
                )
                |> Array.head

            // Valid the signature of the loader function
            do! checkSignature loaderInvokeFunc

            // Execute the loader
            loaderInvokeFunc.Invoke(
                loaderExpr,
                [|
                    box context
                |]
            )
            |> ignore

            return! Ok()
        }
        |> Result.mapError (fun error ->
            [
                $"Invalid loader: {loaderPath}"
                $"Error: {error}"
                """
Example:

open FSharp.Static.Core

let loader (projectRoot: ProjectRoot.T) (context: Context) =
    // Your code goes here
    ()"""
            ]
            |> String.concat "\n"
        )
