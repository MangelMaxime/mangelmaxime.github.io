#r "../src/Nacara/bin/Debug/net6.0/Nacara.Core.dll"
#load "../.paket/load/net6.0/Docs/docs.group.fsx"
#load "../utils/Log.fsx"

open FsConfig
open Nacara.Core

type Env =
    | Dev
    | Prod

[<Convention("FORNAX")>]
type EnvInfo =
    {
        Env: Env
    }

let loader (context: Context) : unit =

    // match EnvConfig.Get<EnvInfo>() with
    // | Ok config -> siteContext.Add config

    // | Error error ->
    //     match error with
    //     | NotFound envVarName ->
    //         siteContext.LogError
    //             $"Environment variable %s{envVarName} not found"
    //     | BadValue (envVarName, value) ->
    //         siteContext.LogError
    //             $"Environment variable %s{envVarName} has invalid value: %s{value}"
    //     | NotSupported msg ->
    //         siteContext.LogError $"Environment variable not supported: %s{msg}"

    //     System.Environment.Exit(1)

    ()
