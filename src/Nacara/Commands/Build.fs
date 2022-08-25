module Nacara.Commands.Build

open Nacara
open Nacara.Core
open Nacara.Evaluator
open System
open System.IO

let execute () =
    let context = Shared.loadConfigOrExit ()

    // Clean artifacts from previous builds
    if Directory.Exists(AbsolutePath.toString context.OutputPath) then
        Directory.Delete(AbsolutePath.toString context.OutputPath, true)

    // Ensure that the output directory exists.
    context.OutputPath
    |> AbsolutePath.toString
    |> Directory.CreateDirectory
    |> ignore

    let (validPages, erroredPages) =
        Shared.extractFiles context

    // Store the valid pages in the context
    validPages
    |> Seq.iter context.Add

    // Some page are errored report the errors and stop.
    if erroredPages.Length > 0 then
        erroredPages |> Array.iter Log.error

        1
    else

        let mutable hasPageGenerationError = false
        validPages
        |> Array.iter (fun pageContext ->
            // Generate the page, if it fails stop.
            let pageGenerationResult =
                Shared.renderPage context pageContext

            if not hasPageGenerationError && pageGenerationResult then
                hasPageGenerationError <- true
        )

        if hasPageGenerationError then
            1
        else
            0
