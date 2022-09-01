module Nacara.Commands.Watch

open Nacara
open Nacara.Core
open Nacara.Evaluator
open System
open System.IO
open Spectre.Console
open System.Diagnostics
open FSharp.Compiler.Interactive.Shell
open Saturn
open Nacara.Server
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Hosting
open Microsoft.AspNetCore.Builder
open AspNetCore.NacaraLoggerExtensions

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
    | RendererChanged of AbsolutePath.T

let private createSourceWatcher
    (onChange: AbsolutePath.T -> unit)
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

                    onChange (AbsolutePath.create fileChange.FullPath)

                | _ ->
                    Log.debug
                        $"File status not supported yet: %A{fileChange.Status}"
            )
        )

let private createRendererDepencyWatcher
    (info: DependencyWatchInfo)
    (onChange: AbsolutePath.T -> unit)
    =

    Watcher.createWithFilters
        (AbsolutePath.getDirectoryName info.DependencyPath)
        [
            AbsolutePath.getFileName info.DependencyPath
        ]
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
                    [
                        $"Dependency of a renderer changed"
                        $"    Dependency: \"%s{AbsolutePath.toString info.DependencyPath}\""
                        $"    Renderer: \"%s{AbsolutePath.toString info.RendererPath}\""
                    ]
                    |> String.concat "\n"
                    |> Log.info

                    onChange info.RendererPath
                | _ ->
                    Log.debug
                        $"File status not supported yet: %A{fileChange.Status}"
            )
        )

let private createRendererWatcher
    (onChange: AbsolutePath.T -> unit)
    (absolutePath: AbsolutePath.T)
    =

    Watcher.createWithFilters
        (AbsolutePath.getDirectoryName absolutePath)
        [
            AbsolutePath.getFileName absolutePath
        ]
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
                        $"Renderer changed \"%s{fileChange.FullPath}\": %A{fileChange.Status}"

                    onChange (AbsolutePath.create fileChange.FullPath)

                | _ ->
                    Log.debug
                        $"File status not supported yet: %A{fileChange.Status}"
            )
        )

let private runSetup
    (fsi: FsiEvaluationSession)
    (context: Context)
    (registerDependencyForWatch: DependencyWatchInfo -> unit)
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
        Shared.renderPage fsi context pageContext (Some registerDependencyForWatch)
        |> ignore
    )

    sw.Stop()
    Log.info $"Site generated in %i{sw.ElapsedMilliseconds} ms"

let private createLocalServer (context: Context) =
    application {
        url $"http://localhost:%i{context.Config.Port}"
        no_router
        use_static (AbsolutePath.toString context.OutputPath)

        logging (fun builder ->
            builder.ClearProviders().AddNacaraLogger() |> ignore

            builder.SetMinimumLevel LogLevel.Warning |> ignore
        )

        app_config (fun builder ->
            builder
                .UseWebSockets()
                .UseMiddleware<LiveReloadWebSockets.LiveReloadWebSocketMiddleware>()
        )
    }

let private cleanState
    (fsi: FsiEvaluationSession)
    (localServer: byref<IHost>)
    (context: Context)
    (watchers: ResizeArray<IDisposable>)
    =

    // Dispose the previous watchers
    watchers |> Seq.iter (fun watcher -> watcher.Dispose())
    watchers.Clear()
    // Clear the renderer cache
    // Is this really needed? For now, I will leave it here
    // because it is not hurting the performance too much
    RendererEvaluator.clearCache ()
    // Dispose the previous server
    localServer.StopAsync() |> Async.AwaitTask |> Async.RunSynchronously
    // Start the new server
    localServer <- (createLocalServer context).Build()

let private runServer (context: Context) (server: IHost) =
    server.RunAsync() |> Async.AwaitTask |> Async.StartImmediate
    Log.info $"Server started at: http://localhost:%i{context.Config.Port}"

let private onConfigChange
    (fsi: FsiEvaluationSession)
    (context: byref<Context>)
    (localServer: byref<IHost>)
    (watchers: ResizeArray<IDisposable>)
    (onSourceChange: AbsolutePath.T -> unit)
    (onRenderedChanged: AbsolutePath.T -> unit)
    (registerDependencyForWatch: DependencyWatchInfo -> unit)
    =
    AnsiConsole.Clear()
    Log.info "Configuration changed - Restarting..."
    // 1. Create a new context
    let newContext = Shared.createContext ()
    context <- newContext
    // 2. Load the new configuration
    Shared.loadConfigOrExit fsi newContext
    // 3. Clean the different memorized states
    cleanState fsi &localServer newContext watchers
    // 4. Re-run the setup phase because the config has changed
    runSetup fsi newContext registerDependencyForWatch
    // 5. Start the new server, because now the new files are available
    runServer newContext localServer
    // 6. Notify the client that the site has been updated
    LiveReloadWebSockets.notifyClientsToReload ()
    // 7. Register the watchers
    watchers.Add(createSourceWatcher onSourceChange newContext)

    newContext.Config.Render
    |> List.iter (fun renderer ->
        let absolutePath =
            Path.Combine(
                ProjectRoot.toString newContext.ProjectRoot,
                renderer.Script
            )
            |> AbsolutePath.create

        watchers.Add(createRendererWatcher onRenderedChanged absolutePath)
    )

let private onSourceFileChanged
    (fsi: FsiEvaluationSession)
    (context: Context)
    (registerDependencyForWatch: DependencyWatchInfo -> unit)
    (pathOfChangedFile: AbsolutePath.T)
    =

    match Shared.extractFile context pathOfChangedFile with
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
            Shared.renderPage fsi context pageContext (Some registerDependencyForWatch)
            |> ignore
        )

        sw.Stop()
        Log.info $"Site generated in %i{sw.ElapsedMilliseconds} ms"

        LiveReloadWebSockets.notifyClientsToReload ()

    | Error errorMessage -> Log.error errorMessage

let execute () =
    let mutable context = Shared.createContext ()
    use fsi = EvaluatorHelpers.fsi context
    Shared.loadConfigOrExit fsi context

    let mutable server = (createLocalServer context).Build()

    let watchers = ResizeArray()
    let registeredDependencyForWatchCache = ResizeArray()

    let rec registerDependencyForWatch (info: DependencyWatchInfo) =
        if not (registeredDependencyForWatchCache.Contains info) then
            let onChange (pathOfChangedFile : AbsolutePath.T) =
                RendererEvaluator.removeItemFromCache pathOfChangedFile
                runSetup fsi context registerDependencyForWatch

            registeredDependencyForWatchCache.Add info
            watchers.Add(createRendererDepencyWatcher info onChange)

    let onSourceFileChanged =
        onSourceFileChanged fsi context registerDependencyForWatch

    let onRenderedChanged (pathOfChangedFile: AbsolutePath.T) =
        RendererEvaluator.removeItemFromCache pathOfChangedFile
        runSetup fsi context registerDependencyForWatch

    // Watch nacara.fsx file for changes
    use _ =
        Watcher.createWithFilters
            (ProjectRoot.toString context.ProjectRoot)
            [
                "nacara.fsx"
            ]
            (fun _ ->
                onConfigChange
                    fsi
                    &context
                    &server
                    watchers
                    onSourceFileChanged
                    onRenderedChanged
                    registerDependencyForWatch
            )

    // 1. Run the setup phase
    runSetup fsi context registerDependencyForWatch
    // 2. Start the server
    runServer context server
    // 3. Notify clients to reload, it can happen that the user
    // has a browser tab open and he killed then restarted the server manually
    LiveReloadWebSockets.notifyClientsToReload ()
    // 4. Register the watchers
    watchers.Add(createSourceWatcher onSourceFileChanged context)

    context.Config.Render
    |> List.iter (fun renderer ->
        let absolutePath =
            Path.Combine(
                ProjectRoot.toString context.ProjectRoot,
                renderer.Script
            )
            |> AbsolutePath.create

        watchers.Add(createRendererWatcher onRenderedChanged absolutePath)
    )

    keepAlive ()
    0
