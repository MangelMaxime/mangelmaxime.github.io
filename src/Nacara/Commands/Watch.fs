module Nacara.Commands.Watch

open Nacara
open Nacara.Core
open Nacara.Evaluator
open System
open System.IO
open Spectre.Console

let private keepAlive () =

    // Keep the program alive until the user presses Ctrl+C
    Console.CancelKeyPress.AddHandler(fun _ ea ->
        ea.Cancel <- true
        Log.info "Received Ctrl+C, shutting down..."
        exit 0
    )

    while true do
        Console.ReadKey(true) |> ignore

type NacaraMsg =
    | ConfigChanged


let execute () =
    let context = Shared.loadConfigOrExit ()

    // Clean artifacts from previous builds
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

    // Report the errors and continue because we are in watch mode
    erroredPages |> Array.iter Log.error

    // Render the pages
    validPages
    |> Array.iter (fun pageContext ->
        Shared.renderPage context pageContext
        |> ignore
    )

    // Setup the watchers
    use configWatcher =
        Watcher.createWithFilters
            (ProjectRoot.toString context.ProjectRoot)
            [
                "nacara.fsx"
            ]
            (fun changes ->
                changes
                |> Seq.iter (fun _ ->
                    AnsiConsole.Clear()
                    Log.info "Configuration changed - Restarting..."


                )
            )

    keepAlive ()
    0
