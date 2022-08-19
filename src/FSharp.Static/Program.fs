module FSharp.Static.Main

open Argu
open System
// open Suave
// open Suave.Operators
// open Suave.Filters
// open Suave.Successful
// open Suave.RequestErrors
// open Suave.Logging
open System.IO
open Spectre.Console
open Saturn
open Giraffe
open Microsoft.Extensions.Logging
open FSharp.Static.Server
open FSharp.Static.Core
open System.Diagnostics
open FSharp.Static.Evaluator

// open Suave.Sockets
// open Suave.Sockets.Control
// open Suave.WebSocket

let private handleWebsocketReload () = ""

let private signalUpdate = new Event<unit>()

[<CliPrefix(CliPrefix.None)>]
type CliArguments =
    | Build
    | Version

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Build -> "Build the website."
            | Version -> "Print the version."

/// Minor optimization for Argu
/// See: https://fsprojects.github.io/Argu/perf.html
let inline private checkStructure<'T> =
#if DEBUG
    true
#else
    false
#endif

let private createFileWatch (root: string) handler =
    let watcher = new FileSystemWatcher()
    watcher.Path <- root
    watcher.EnableRaisingEvents <- true
    watcher.IncludeSubdirectories <- true

    watcher.NotifyFilter <-
        NotifyFilters.DirectoryName
        ||| NotifyFilters.LastWrite
        ||| NotifyFilters.FileName

    watcher.Created.Add handler
    watcher.Changed.Add handler
    watcher.Deleted.Add handler

    watcher

// open Microsoft.Extensions.Logging

let app =
    application {
        url "http://localhost:8080"
        use_router (text "Hello world from Saturn")

        logging (fun builder ->
            builder
                .ClearProviders()
                .AddColorConsoleLogger(fun configuration ->
                    // Replace warning value from appsettings.json of "Cyan"
                    configuration.LogLevels[
                        LogLevel.Warning
                    ] <- ConsoleColor.DarkCyan
                    // Replace warning value from appsettings.json of "Red"
                    configuration.LogLevels[
                        LogLevel.Error
                    ] <- ConsoleColor.DarkRed
                )
            |> ignore
        // logger.SetMinimumLevel LogLevel.None |> ignore
        )
    }

let private loadLoaders (projectRoot : ProjectRoot.T) (context : Context) =

    let loaders =
        Directory.GetFiles(
            Path.Combine(ProjectRoot.toString projectRoot, "loaders"),
            "*.fsx"
        )
        |> Seq.map AbsolutePath.create

    // Stop on first error
    for loader in loaders do
        match LoaderEvaluator.tryEvaluate loader context with
        | Ok () ->
            ()
        | Error error ->
            Log.error error

[<EntryPoint>]
let main argv =
    let errorHandler =
        ProcessExiter(
            colorizer =
                function
                | ErrorCode.HelpText -> None
                | _ -> Some ConsoleColor.Red
        )

    let parser =
        ArgumentParser.Create<CliArguments>(
            programName = "fstatic",
            errorHandler = errorHandler,
            checkStructure = checkStructure
        )

    let results = parser.ParseCommandLine(inputs = argv).GetAllResults()

    // if results.Length = 0 then

    // Server.create 8080 signalUpdate |> Async.AwaitTask |> Async.StartImmediate
    // async { run app } |> Async.Start

    let projectRoot = ProjectRoot.create (Directory.GetCurrentDirectory())

    Log.info $"CWD: {ProjectRoot.toString projectRoot}"

    let context = new Context(projectRoot, true, Log.error)

    use configWatcher =
        Watcher.createWithFilters
            (ProjectRoot.toString projectRoot)
            [
                "config.fsx"
            ]
            (fun changes ->
                changes
                |> Seq.iter (fun _ ->
                    AnsiConsole.Clear()
                    Log.info "Configuration changed - Restarting..."

                    match ConfigEvaluator.tryEvaluate context with
                    | Ok config ->
                        Log.debug "ok config"
                        context.Add config
                    | Error error -> Log.error error

                )
            )

    match ConfigEvaluator.tryEvaluate context with
    | Ok config ->
        Log.info "Config loaded"
        context.Add config

        loadLoaders projectRoot context


    | Error error -> Log.error error

    // Keep the program alive until the user presses Ctrl+C
    Console.CancelKeyPress.AddHandler(fun _ ea ->
        ea.Cancel <- true
        Log.info "Received Ctrl+C, shutting down..."
        Environment.Exit(0)
    )

    while true do
        System.Console.ReadKey(true) |> ignore

    0
