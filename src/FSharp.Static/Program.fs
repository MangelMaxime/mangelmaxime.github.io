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
open System.Reactive
open System.Reactive.Linq
open System.Reactive.Subjects
open Saturn
open Giraffe
open Microsoft.Extensions.Logging
open System.Collections.Generic

let x = new Subject<string>()

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

type ColorConsoleLoggerConfiguration() =

    let _logLevels = new Dictionary<LogLevel, ConsoleColor>()

    do _logLevels.Add(LogLevel.Information, ConsoleColor.Green)

    member _.EventId: int = unbox null

    member _.LogLevels: Dictionary<LogLevel, ConsoleColor> = _logLevels


type MyLogger
    (
        name: string,
        getCurrentConfig: Func<unit, ColorConsoleLoggerConfiguration>
    ) =

    member _.IsEnabled(logLevel: LogLevel) =
        getCurrentConfig.Invoke().LogLevels.ContainsKey(logLevel)

    interface ILogger with

        member _.BeginScope<'TState>(state: 'TState) = null

        member this.IsEnabled(logLevel: LogLevel) = this.IsEnabled logLevel

        member this.Log<'TState>
            (
                logLevel: LogLevel,
                eventId: EventId,
                state: 'TState,
                exn: Exception,
                formatter: Func<'TState, Exception, string>
            ) =
            if this.IsEnabled(logLevel) then
                let config = getCurrentConfig.Invoke()

                if (config.EventId = 0 || config.EventId = eventId.Id) then
                    let originalColor = Console.ForegroundColor

                    Console.ForegroundColor <- config.LogLevels[logLevel]
                    Console.WriteLine($"[{eventId.Id, 2}: {logLevel, -12}]")

                    Console.ForegroundColor <- originalColor
                    Console.Write($"     {name} - ")

                    Console.ForegroundColor <- config.LogLevels[logLevel]
                    Console.Write($"{formatter.Invoke(state, exn)}")

                    Console.ForegroundColor <- originalColor
                    Console.WriteLine()

            else
                ()


let app =
    application {
        url "http://localhost:8080"
        use_router (text "Hello world from Saturn")
        logging (fun builder ->
            // builder.ClearProviders()
            //     .AddColorConsoleLogger(con)
            // // logger.SetMinimumLevel LogLevel.None |> ignore
            ()
        )
    }

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
    async { run app } |> Async.Start

    printfn "%A" (Directory.GetCurrentDirectory())
    printfn "New file"

    use configWatcher =
        // createFileWatch (Directory.GetCurrentDirectory()) (fun event ->
        //     printfn $"%A{event.ChangeType} - %A{event.FullPath} - %A{event.Name}"
        // )
        Watcher.createWithFilters
            (Directory.GetCurrentDirectory())
            [
                "config.fsx"
            ]
            (fun changes ->
                changes
                |> Seq.iter (fun _ ->
                    AnsiConsole.Clear()
                    Log.info "Configuration changed - Restarting..."
                )
            )

    // Keep the program alive until the user presses Ctrl+C
    Console.CancelKeyPress.AddHandler(fun _ ea ->
        ea.Cancel <- true
        Log.info "Received Ctrl+C, shutting down..."
        Environment.Exit(0)
    )

    while true do
        System.Console.ReadKey(true) |> ignore
        printfn "fake trigger"
        signalUpdate.Trigger()

    0
