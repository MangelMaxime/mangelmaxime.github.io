namespace Nacara.Evaluator

open FsToolkit.ErrorHandling
open Nacara.Core
open System.Reflection
open System.Collections.Concurrent
open FSharp.Compiler.Interactive.Shell

[<RequireQualifiedAccess>]
module RendererEvaluator =

    type private CacheData =
        {
            Method: MethodInfo
            Expression: obj
        }

    let private cache = new ConcurrentDictionary<_, _>()

    let clearCache () = cache.Clear()

    let removeItemFromCache (key: AbsolutePath.T) =
        cache.TryRemove(key) |> ignore

    let private checkSignature (func: MethodInfo) =
        let parameters = func.GetParameters()

        result {
            do!
                EvaluatorHelpers.checkParameterType
                    1
                    parameters[0]
                    Typeof.context

            do!
                EvaluatorHelpers.checkParameterType
                    2
                    parameters[1]
                    Typeof.pageContext

            if func.ReturnType = Typeof.string then
                return! Ok()
            else
                return! Error "Render function should return 'string'"
        }

    let private loadRenderer
        fsi
        (renderPath: AbsolutePath.T)
        =
        result {
            do! EvaluatorHelpers.tryLoad fsi renderPath
            do! EvaluatorHelpers.tryOpen fsi renderPath

            let fileName = AbsolutePath.toString renderPath
            let source =
                """
#r "../src/Nacara/bin/Debug/net6.0/Nacara.Core.dll"
#load "./layout.fsx"
#load "../.paket/load/net6.0/Docs/docs.group.fsx"

// open Giraffe.ViewEngine
open type Feliz.ViewEngine.Html
open Helpers
open Nacara.Core

let render (ctx: Context) (page: PageContext) =

    div [ rawText (RelativePath.toString page.RelativePath) ] |> Layout.mainPage ctx
                """
                |> FSharp.Compiler.Text.SourceText.ofString

            let opts, errors =
                fsi.InteractiveChecker.GetProjectOptionsFromScript(fileName, source)
                |> Async.RunSynchronously

            opts.SourceFiles
            |> Seq.iter (fun file ->
                printfn "%A" file
            )

            let! renderFunc =
                EvaluatorHelpers.tryEvaluateCode
                    fsi
                    renderPath
                    "<@@ fun a b -> render a b @@>"

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

            return
                {
                    Method = renderInvokeFunc
                    Expression = renderExpr
                }
        }

    let tryEvaluate
        (fsi: FsiEvaluationSession)
        (rendererPath: AbsolutePath.T)
        (context: Context)
        (pageContext: PageContext)
        =
        result {
            let! (renderInfo: CacheData) =
                match cache.TryGetValue rendererPath with
                | true, renderInvokeFunc -> renderInvokeFunc

                | false, _ ->
                    cache.AddOrUpdate(
                        rendererPath,
                        loadRenderer fsi rendererPath,
                        fun _ renderInvokeFunc -> renderInvokeFunc
                    )

            // Execute the render
            let untypedResult =
                renderInfo.Method.Invoke(
                    renderInfo.Expression,
                    [|
                        box context
                        box pageContext
                    |]
                )

            match tryUnbox<string> untypedResult with
            | Some result -> return! Ok result

            | None -> return! Error "Render function should return 'string'"
        }
        |> Result.mapError (fun error ->
            [
                $"Invalid renderer: {AbsolutePath.toString rendererPath}"
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
