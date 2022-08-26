module Nacara.Commands.Watch

open Nacara
open Nacara.Core
open Nacara.Evaluator
open System
open System.IO
open Spectre.Console
open System.Diagnostics
open FSharp.Compiler.Interactive.Shell

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
    | RunSetup
    | SourceFileChanged of AbsolutePath.T

let createSourceWatcher
    (agent: MailboxProcessor<NacaraMsg>)
    (context: Context)
    =
    Watcher.create
        (AbsolutePath.toString context.SourcePath)
        (fun changes ->
            changes
            |> Seq.iter (fun fileChange ->
                [
                    // Adding some spaces for easier readability
                    ""
                    ""
                    "Change detected, rebuilding site."
                ]
                |> String.concat "\n"
                |> Log.info
                // signalUpdate.raise()
                match fileChange.Status with
                | Watcher.Changed ->
                    Log.info
                        $"Source changed \"%s{fileChange.FullPath}\": %A{fileChange.Status}"

                    agent.Post(
                        SourceFileChanged(
                            AbsolutePath.create fileChange.FullPath
                        )
                    )

                | _ ->
                    Log.debug
                        $"File status not supported yet: %A{fileChange.Status}"
            )
        )

let private runSetup
    (agent: MailboxProcessor<NacaraMsg>)
    (fsi: FsiEvaluationSession)
    (context: Context)
    =
    let sw = Stopwatch.StartNew()
    // Clean artifacts from previous builds
    Directory.Delete(AbsolutePath.toString context.OutputPath, true)

    // Ensure that the output directory exists.
    context.OutputPath
    |> AbsolutePath.toString
    |> Directory.CreateDirectory
    |> ignore

    Log.info "Generating site..."

    let (validPages, erroredPages) = Shared.extractFiles context

    // Store the valid pages in the context
    // Like that the pages can be accessed when during the rendering process
    // Example: To generate some navigation
    validPages |> Seq.iter context.Add

    // Report the errors and continue because we are in watch mode
    erroredPages |> Array.iter Log.error

    // Render the pages
    validPages
    |> Array.iter (fun pageContext ->
        Shared.renderPage fsi context pageContext |> ignore
    )

    sw.Stop()
    Log.info $"Site generated in %i{sw.ElapsedMilliseconds} ms"

/// <summary>
/// The agent loop responsible for handling Watch mode of Nacara
///
/// <note>
/// This function is declared outside of the Mailbox creation in order to avoid
/// having access to scoped variables.
/// </note>
/// </summary>
/// <param name="agent">The agent used to listen or send messages</param>
/// <param name="fsi">FSI instance used for evaluatin F# code</param>
/// <param name="context">Current Nacara context</param>
/// <param name="sourceWatcher">Current instance of the source watcher. Like that we can dispose it when needed</param>
/// <typeparam name="'a"></typeparam>
/// <returns>
/// Returns nothing as the agent is always running unless the user presses Ctrl+C
/// to exit the program.
/// </returns>
let rec private agentLoop
    (agent: MailboxProcessor<NacaraMsg>)
    (fsi: FsiEvaluationSession)
    (context: Context)
    (sourceWatcher: IDisposable option)
    =
    async {
        let! msg = agent.Receive()

        try
            match msg with
            | ConfigChanged ->
                let newContext = Shared.createContext ()
                Shared.loadConfigOrExit fsi context

                agent.Post RunSetup
                return! agentLoop agent fsi newContext None

            | RunSetup ->
                // Should we empty the agent queue?

                // Dispose the previous source watcher
                match sourceWatcher with
                | Some sourceWatcher -> sourceWatcher.Dispose()
                | None -> ()

                runSetup agent fsi context

                let sourceWatcher = createSourceWatcher agent context

                return! agentLoop agent fsi context (Some sourceWatcher)

            | SourceFileChanged file ->
                match Shared.extractFile context file with
                | Ok pageContext ->
                    Log.info "Generating site..."
                    let sw = Stopwatch.StartNew()

                    let newPagesInMemory =
                        match context.TryGetValues<PageContext>() with
                        | Some knownPages ->
                            knownPages
                            |> Seq.map (fun currentPageContext ->
                                // If the page context is the same as the one we want to update
                                if
                                    currentPageContext.AbsolutePath = pageContext.AbsolutePath
                                then
                                    pageContext
                                else
                                    currentPageContext
                            )
                            |> ResizeArray

                        | None ->
                            ResizeArray
                                [
                                    pageContext
                                ]

                    context.Replace newPagesInMemory

                    newPagesInMemory
                    |> Seq.iter (fun pageContext ->
                        Shared.renderPage fsi context pageContext |> ignore
                    )

                    sw.Stop()
                    Log.info $"Site generated in %i{sw.ElapsedMilliseconds} ms"

                    return! agentLoop agent fsi context sourceWatcher

                | Error errorMessage ->
                    Log.error errorMessage
                    return! agentLoop agent fsi context sourceWatcher

        with ex ->
            Log.error "An unexpected error occurred"
            AnsiConsole.WriteException ex
            // Keep the loop going
            return! agentLoop agent fsi context sourceWatcher
    }

let execute () =
    let initialContext = Shared.createContext ()
    use fsi = EvaluatorHelpers.fsi initialContext
    Shared.loadConfigOrExit fsi initialContext

    let nacaraAgent =
        MailboxProcessor.Start(fun inbox ->
            agentLoop inbox fsi initialContext None
        )

    // Config watcher is at the top level so it reset Nacara when the config changes
    use _ =
        Watcher.createWithFilters
            (ProjectRoot.toString initialContext.ProjectRoot)
            [
                "nacara.fsx"
            ]
            (fun changes ->
                changes
                |> Seq.iter (fun _ ->
                    AnsiConsole.Clear()
                    Log.info "Configuration changed - Restarting..."
                    nacaraAgent.Post ConfigChanged
                )
            )

    nacaraAgent.Post RunSetup

    keepAlive ()
    0
