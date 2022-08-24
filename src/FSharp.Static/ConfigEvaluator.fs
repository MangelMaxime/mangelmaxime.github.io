namespace FSharp.Static.Evaluator

open FsToolkit.ErrorHandling
open FSharp.Static.Core

[<RequireQualifiedAccess>]
module ConfigEvaluator =

    let tryEvaluate (context : Context) =
        use fsi = EvaluatorHelpers.fsi context

        result {
            do! EvaluatorHelpers.tryLoad fsi "config.fsx"
            do! EvaluatorHelpers.tryOpen fsi "config.fsx"

            let! configValue = EvaluatorHelpers.tryEvaluateCode fsi "config.fsx" "config"

            if configValue.ReflectionType <> Typeof.config then
                return! Error """Invalid config type detected. Please make sure that your 'config.fsx' file contains a 'config' variable of type 'FSharp.Static.Core.Config'

Example:
#r "./src/FSharp.Static/bin/Debug/net6.0/FSharp.Static.Core.dll"

open FSharp.Static.Core

let config =
    {
        Directory =
            {
                Input = "docsrc"
                Output = "public"
                Generators = "generators"
                Loaders = "loaders"
            }
    }
                """

            else
                return! Ok (configValue.ReflectionValue :?> Config)
        }