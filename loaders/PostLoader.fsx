#r "../_lib/Fornax.Core.dll"
#load "../.paket/load/main.group.fsx"
#load "../utils/Log.fsx"
#load "../utils/Helpers.fsx"

open System
open System.IO
open System.Diagnostics
open System.Threading.Tasks
open Legivel.Serialization
open Helpers

type Post =
    {
        relativeFile: string
        link : string
        title: string
        author: string option
        published: DateTime option
        tags: string list
        content: string
    }

type PostFrontMatter =
    {
        layout : string
        title: string
        author: string option
        published: DateTime option
        tags: string list
    }

let contentDir = "posts"

let private getLastModified (fileName: string) =
    async {
        let psi = ProcessStartInfo()
        psi.FileName <- "git"
        psi.Arguments <- $"--no-pager log -1 --format=%%ai \"%s{fileName}\""
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
            // File is not in the git repo
            if String.IsNullOrEmpty result[0] then
                return DateTime.Now
            else
                return DateTime.Parse(result[0])
        else
            Log.error $"Failed to get last modified information %s{result[1]}"
            return DateTime.Now
    }
    |> Async.RunSynchronously


let private loadFile (rootDir: string) (absolutePath: string) =
    let text = File.ReadAllText absolutePath

    let relativePath =
        Path.relativePath rootDir absolutePath

    let lines = text.Replace("\r\n", "\n").Split("\n")

    let x = getLastModified absolutePath

    let firstLine = Array.tryHead lines

    if firstLine <> Some "---" then
        Log.error $"File '%s{relativePath}' does not have a front matter"
        None

    else
        let lines = lines |> Array.skip 1

        let frontMatterLines =
            lines
            |> Array.takeWhile (fun line -> line <> "---")

        let markdownContent =
            lines
            |> Array.skip (frontMatterLines.Length + 1)
            |> String.concat "\n"

        let frontMatterContent = frontMatterLines |> String.concat "\n"

        let frontMatterResult =
            Deserialize<PostFrontMatter> frontMatterContent
            |> List.head

        match frontMatterResult with
        | Error error ->
            Log.error $"Error parsing front matter in file '%s{relativePath}': %A{error}"
            None

        | Success frontMatter ->
            if not (frontMatter.Warn.IsEmpty) then
                for warning in frontMatter.Warn do
                    Log.warn $"Warning in file '%s{relativePath}': %A{warning}"

            let link =
                Path.ChangeExtension(relativePath, "html")

            {
                relativeFile = relativePath
                link = link
                title = ""
                author = None
                published = frontMatter.Data.published
                tags = frontMatter.Data.tags
                content = markdownContent
            }
            |> Some

let loader (projectRoot: string) (siteContent: SiteContents) =
    let postsPath = Path.Combine(projectRoot, contentDir)
    let options = EnumerationOptions(RecurseSubdirectories = true)
    let files = Directory.GetFiles(postsPath, "*", options)

    files
    |> Array.filter (fun n -> n.EndsWith ".md")
    |> Array.map (loadFile projectRoot)
    // Only keep the valid post to avoid to propagate errors
    |> Array.filter Option.isSome
    |> Array.map Option.get
    |> Array.iter siteContent.Add

    siteContent
