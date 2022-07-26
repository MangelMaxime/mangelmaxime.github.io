#r "../_lib/Fornax.Core.dll"
#load "../utils/Log.fsx"

open System.Diagnostics
open System.Threading.Tasks

let generate (ctx: SiteContents) (projectRoot: string) (page: string) =

    async {
        let psi = ProcessStartInfo()
        psi.FileName <- "dart-sass/linux-x64/sass"
        psi.Arguments <- page
        psi.RedirectStandardError <- true
        psi.RedirectStandardOutput <- true
        psi.CreateNoWindow <- true
        psi.WindowStyle <- ProcessWindowStyle.Hidden
        psi.UseShellExecute <- false

        use p = new Process()
        p.StartInfo <- psi
        p.Start() |> ignore

        let outTask =
            Task.WhenAll(
                [|
                    p.StandardOutput.ReadToEndAsync()
                    p.StandardError.ReadToEndAsync()
                |]
            )

        do! p.WaitForExitAsync() |> Async.AwaitTask
        let! result = outTask |> Async.AwaitTask

        if p.ExitCode = 0 then
            return result[0]
        else
            Log.error result[1]
            return $"SASS generation failed %s{result[1]}"
    }
    |> Async.RunSynchronously
