module Nacara.Commands.Shared

open Nacara
open Nacara.Core
open Nacara.Evaluator
open System
open System.IO

let loadConfigOrExit () =

    let projectRoot =
        ProjectRoot.create (Directory.GetCurrentDirectory())

    Log.info $"CWD: {ProjectRoot.toString projectRoot}"

    let context =
        Context(projectRoot, false, Log.error)

    let configPath =
        Path.Combine(ProjectRoot.toString projectRoot, "nacara.fsx")

    if File.Exists configPath then

        match ConfigEvaluator.tryEvaluate context with
        | Ok config ->
            Log.info "Config loaded"

            let config =
                { config with
                    // Remove template config duplicates
                    Templates = List.distinct config.Templates
                }

            context.Add config

            context

        // Invalid config: Report and exit
        | Error errorMessage ->
            Log.error errorMessage
            Environment.Exit(1)
            failwith "Not reachable"

    else
        Log.error $"Config file not found at '{configPath}'"
        Environment.Exit(1)
        failwith "Not reachable"
