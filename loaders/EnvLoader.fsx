#r "C:\\Users\\mange\\Workspaces\\Github\\ionide\\Fornax\\src\\Fornax\\bin\\Debug\\net5.0\\Fornax.Core.dll"
#load "../.paket/load/main.group.fsx"
#load "../utils/Log.fsx"

open FsConfig

type Env =
    | Dev
    | Prod

[<Convention("FORNAX")>]
type EnvInfo = {
    Env: Env
}

let loader (projectRoot: string) (siteContent: SiteContents) =

    match EnvConfig.Get<EnvInfo>() with
    | Ok config -> siteContent.Add config

    | Error error ->
        match error with
        | NotFound envVarName -> Log.error $"Environment variable %s{envVarName} not found"
        | BadValue (envVarName, value) -> Log.error $"Environment variable %s{envVarName} has invalid value: %s{value}"
        | NotSupported msg -> Log.error $"Environment variable not supported: %s{msg}"

        System.Environment.Exit(1)

    siteContent
