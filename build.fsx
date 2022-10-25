#r "nuget: Fun.Build, 0.1.9"

open Fun.Build
open System
open System.IO

let args = Environment.GetCommandLineArgs()

let isHelp = Seq.contains "--help" args || Seq.contains "help" args

if isHelp then
    printfn
        """
Usage: dotnet fsi build.fsx [options]
Options:
    --watch, watch      Start a local web server and watch for changes
    --build, build      Build a production version of the site
    --help, help        Show this help
    """

    exit 0

module Directory =

    let deleteIfExists path =
        if Directory.Exists path then
            Directory.Delete(path, true)

pipeline "Site" {
    workingDir __SOURCE_DIRECTORY__

    stage "Clean" {
        run (fun _ ->
            Directory.deleteIfExists "./_site"
            Directory.deleteIfExists "./_11ty/_js/"
        )
    }

    stage "Watch" {
        whenAny {
            cmdArg "watch"
            cmdArg "--watch"
        }

        envVars
            [
                "ELEVENTY_ENV", "dev"
            ]

        // First compile the TypeScript, so when Eleventy starts, it can find the JS files
        run "npx tsc"

        stage "Paralell" {
            paralle

            stage "TypeScript" { run "npx tsc --watch --preserveWatchOutput" }
            stage "Eleventy" { run "npx @11ty/eleventy --serve" }
        }

    }

    stage "Build" {
        whenAny {
            cmdArg "build"
            cmdArg "--build"
        }

        envVars
            [
                "ELEVENTY_ENV", "prod"
            ]

        run "npx tsc"
        run "npx @11ty/eleventy"
    }

    runIfOnlySpecified false
}
