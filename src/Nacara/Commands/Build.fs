module Nacara.Commands.Build

open Nacara
open Nacara.Core
open Nacara.Evaluator
open System.IO
open System.Diagnostics

let execute () =
    let sw = Stopwatch.StartNew()

    let context = Shared.createContext()
    use fsi = EvaluatorHelpers.fsi context

    Shared.loadConfigOrExit fsi context

    // Clean artifacts from previous builds
    if Directory.Exists(AbsolutePath.toString context.OutputPath) then
        Directory.Delete(AbsolutePath.toString context.OutputPath, true)

    // Ensure that the output directory exists.
    context.OutputPath
    |> AbsolutePath.toString
    |> Directory.CreateDirectory
    |> ignore

    let (validPages, erroredPages) = Shared.extractFiles context

    // Store the valid pages in the context
    validPages |> Seq.iter context.Add

    // Some page are errored report the errors and stop.
    if erroredPages.Length > 0 then
        erroredPages |> Array.iter Log.error

        1
    else

        let mutable hasPageGenerationError = false

        validPages
        |> Array.iter (fun pageContext ->
            let succeeded = Shared.renderPage fsi context pageContext None

            if not hasPageGenerationError && not succeeded then
                hasPageGenerationError <- true
        )

        sw.Stop()

        if hasPageGenerationError then
            Log.error $"Built with error in %i{sw.ElapsedMilliseconds} ms"
            1
        else
            Log.success $"Built in %i{sw.ElapsedMilliseconds} ms"
            0
