module Nacara.Main

open Argu
open System
open System.IO
open Saturn
open Microsoft.Extensions.Logging
open Nacara.Server
open Nacara.Core
open Nacara.Evaluator
open System.Reflection

module Yaml = Legivel.Serialization

let private handleWebsocketReload () = ""

let private signalUpdate = Event<string>()

/// Minor optimization for Argu
/// See: https://fsprojects.github.io/Argu/perf.html
let inline private checkStructure<'T> =
#if DEBUG
    true
#else
    false
#endif

let createServer (servedFolder: string) =
    application {
        url "http://localhost:8080"
        // use_router (text "Hello world from Saturn")
        use_static servedFolder
        no_router

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

            builder.SetMinimumLevel LogLevel.Warning |> ignore
        )
    }

let private loadLoaders (context: Context) =

    let loaders =
        Directory.GetFiles(
            Path.Combine(ProjectRoot.toString context.ProjectRoot, "loaders"),
            "*.fsx"
        )
        |> Seq.map AbsolutePath.create

    // Stop on first error because if a loader fails to load,
    // the website will not be able to be generated.
    for loader in loaders do
        match LoaderEvaluator.tryEvaluate loader context with
        | Ok () -> ()
        | Error error -> Log.error error

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
            programName = "nacara",
            errorHandler = errorHandler,
            checkStructure = checkStructure
        )

    let results =
        parser.ParseCommandLine(inputs = argv).GetAllResults()

    if List.isEmpty results then
        printfn "No arguments provided."
        printfn $"%s{parser.PrintUsage()}"
        1
    else if List.length results > 1 then
        printfn "More than one command was provided"
        printfn $"%s{parser.PrintUsage()}"
        1
    else

        match List.tryHead results with
        | Some Version ->
            Commands.Version.execute ()

        | Some Build ->
            Commands.Build.execute ()

        | Some Watch -> 0

        | Some Serve -> 0

        | Some Clean ->
            Commands.Clean.execute ()

        | None ->
            printfn $"%s{parser.PrintUsage()}"
            1


// use configWatcher =
//     Watcher.createWithFilters
//         (ProjectRoot.toString projectRoot)
//         [
//             "config.fsx"
//         ]
//         (fun changes ->
//             changes
//             |> Seq.iter (fun _ ->
//                 AnsiConsole.Clear()
//                 Log.info "Configuration changed - Restarting..."
//
//                 match ConfigEvaluator.tryEvaluate context with
//                 | Ok config ->
//                     Log.debug "ok config"
//                     context.Add config
//                 | Error error -> Log.error error
//
//             )
//         )
//
// use sourceWatcher =
//     Watcher.create
//         sourceDirectory
//         (fun changes ->
//             changes
//             |> Seq.iter (fun fileChange ->
//                 Log.info "Source changed - Restarting..."
//                 // signalUpdate.raise()
//                 match fileChange.Status with
//                 | Watcher.Changed ->
//                     Log.debug "changed"
//                     signalUpdate.Trigger(fileChange.FullPath)
//
//                 | _ ->
//                     Log.debug
//                         $"File status not supported yet: %A{fileChange.Status}"
//             )
//         )

// Keep the program alive until the user presses Ctrl+C
// Console.CancelKeyPress.AddHandler(fun _ ea ->
//     ea.Cancel <- true
//     Log.info "Received Ctrl+C, shutting down..."
//     Environment.Exit(0)
// )
//
// while true do
//     Console.ReadKey(true) |> ignore
