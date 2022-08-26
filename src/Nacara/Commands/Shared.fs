module Nacara.Commands.Shared

open Nacara
open Nacara.Core
open Nacara.Evaluator
open System
open System.IO
open System.Diagnostics
open FSharp.Compiler.Interactive.Shell

module Yaml = Legivel.Serialization

let createContext () =

    let projectRoot =
        ProjectRoot.create (Directory.GetCurrentDirectory())

    Log.info $"CWD: {ProjectRoot.toString projectRoot}"

    Context(projectRoot, false, Log.error)


let loadConfigOrExit (fsi : FsiEvaluationSession) (context : Context) =
    let sw = Stopwatch.StartNew()

    let configPath =
        Path.Combine(ProjectRoot.toString context.ProjectRoot, "nacara.fsx")

    if File.Exists configPath then

        match ConfigEvaluator.tryEvaluate fsi with
        | Ok config ->
            sw.Stop()
            Log.info $"Config loaded in %i{sw.ElapsedMilliseconds} ms"

            let config =
                { config with
                    // Remove template config duplicates
                    Templates = List.distinct config.Templates
                }

            context.Add config

        // Invalid config: Report and exit
        | Error errorMessage ->
            Log.error errorMessage
            exit 1
            failwith "Not reachable"

    else
        Log.error $"Config file not found at '{configPath}'"
        exit 1
        failwith "Not reachable"

/// <summary>
/// Try to apply a template config to a file and returns it's content if possible.
/// </summary>
/// <param name="templateConfig">Configuration to test for processing the file</param>
/// <param name="lines">All the lines of the file to process</param>
/// <returns>
/// Returns <c>Ok ...</c> if the file was processed correctly.
///
/// Otherwise, returns <c>Error&lt;string&gt;</c> with an explanation of the error.
/// </returns>
let private tryApplyTemplateConfig
    (templateConfig: TemplateConfig)
    (lines: string array)
    =

    match Array.tryHead lines with
    | Some firstLine ->
        if firstLine = templateConfig.FrontMatter.StartDelimiter then
            let frontMattersLines =
                lines
                |> Array.skip 1 // Skip the delimiter line
                |> Array.takeWhile (fun currentLine ->
                    currentLine
                    <> templateConfig.FrontMatter.EndDelimiter
                )

            let content =
                lines
                // Skip the front matter section
                // + 2: Is to skip the delimiter lines
                |> Array.skip (frontMattersLines.Length + 2)
                |> String.concat "\n"

            let frontMatter =
                frontMattersLines |> String.concat "\n"

            let frontMatterResult =
                Yaml.Deserialize<PageFrontMatter> frontMatter
                |> List.head

            match frontMatterResult with
            | Yaml.Error errorInfo ->
                [
                    "Error while parsing front matter"
                    errorInfo.ToString()
                ]
                |> String.concat "\n"
                |> Error

            | Yaml.Success frontMatterValue ->
                {|
                    Content = content
                    FrontMatter = frontMatter
                    FrontMatterData = frontMatterValue.Data
                |}
                |> Ok

        else
            Error "File should start with a front matter delimiter"

    | None -> Error "File is empty"

/// <summary>
/// Try to apply a list of template configs to a file and returns the first that succeeds.
/// </summary>
/// <param name="templateConfigList"></param>
/// <param name="lines"></param>
/// <param name="accErrors"></param>
/// <typeparam name="'a"></typeparam>
/// <returns>
/// Returns <c>Ok ...</c> if one template config succeeded.
///
/// Otherwise, returns <c>Error&lt;string list&gt;</c> with all the errors.
/// </returns>
let rec tryProcessFileContent
    (templateConfigList: TemplateConfig list)
    (lines: string array)
    (accErrors: string list)
    =

    match templateConfigList with
    | templateConfig :: rest ->
        let result =
            tryApplyTemplateConfig templateConfig lines

        match result with
        | Ok result -> Ok result
        | Error error -> tryProcessFileContent rest lines (error :: accErrors)

    | [] -> Error accErrors

let extractFile (context: Context) (filePath: AbsolutePath.T) =
    let relativePath =
        Path.GetRelativePath(
            ProjectRoot.toString context.ProjectRoot,
            AbsolutePath.toString filePath
        )
        |> RelativePath.create

    let pageId =
        relativePath
        |> RelativePath.toString
        |> Path.GetFileNameWithoutExtension
        |> PageId.create

    let rawText =
        File.ReadAllText(AbsolutePath.toString filePath)

    let lines =
        rawText.Replace("\r\n", "\n").Split("\n")

    let fileExtension =
        Path.GetExtension(AbsolutePath.toString filePath)[1..]

    let templateConfigList =
        context.Config.Templates
        |> List.filter (fun templateConfig ->
            templateConfig.Extension = fileExtension
        )

    match tryProcessFileContent templateConfigList lines [] with
    | Ok fileInfo ->
        {
            AbsolutePath = filePath
            RelativePath = relativePath
            PageId = pageId
            Layout = fileInfo.FrontMatterData.Layout
            RawText = rawText
            FrontMatter = fileInfo.FrontMatter
            Content = fileInfo.Content
        }
        |> Ok

    | Error errors ->
        [
            "Error while processing file: "
            + (AbsolutePath.toString filePath)
            + ":"
            + "\n"
            + String.concat "\n" errors
        ]
        |> String.concat "\n"
        |> Error

let extractFiles (context: Context) =
    let sourceFiles =
        try
            Directory.GetFiles(
                AbsolutePath.toString context.SourcePath,
                "*.*",
                SearchOption.AllDirectories
            )
        with
        | :? DirectoryNotFoundException ->
            Log.error
                $"Source directory not found: %s{AbsolutePath.toString context.SourcePath}"

            [||]
        | ex -> raise ex

    sourceFiles
    |> Array.map AbsolutePath.create
    |> Array.map (fun file -> extractFile context file)
    |> Array.partitionMap (fun page ->
        match page with
        | Ok page -> Choice1Of2 page
        | Error errorMessage -> Choice2Of2 errorMessage
    )

let renderPage (fsi  :FsiEvaluationSession) (context: Context) (pageContext: PageContext) =
    let sw = Stopwatch.StartNew()

    let rendererConfigOpt =
        context.Config.Render
        |> List.tryFind (fun rendererConfig ->
            rendererConfig.Layout = pageContext.Layout
        )

    match rendererConfigOpt with
    | Some rendererConfig ->
        let rendererScript =
            Path.Combine(
                ProjectRoot.toString context.ProjectRoot,
                rendererConfig.Script
            )
            |> AbsolutePath.create

        let pageResult =
            RendererEvaluator.tryEvaluate fsi rendererScript context pageContext

        match pageResult with
        | Ok pageContent ->
            match rendererConfig.OutputAction with
            | ChangeExtension newExtension ->
                let destination =
                    Path.Combine(
                        ProjectRoot.toString context.ProjectRoot,
                        context.Config.Directory.Output,
                        Path.ChangeExtension(
                            PageId.toString pageContext.PageId,
                            newExtension
                        )
                    )
                    |> AbsolutePath.create

                File.WriteAllText(
                    AbsolutePath.toString destination,
                    pageContent
                )

                sw.Stop()

                Log.info
                    $"Generated \"%s{RelativePath.toString pageContext.RelativePath}\" in %i{sw.ElapsedMilliseconds} ms"

                true

        | Error errorMessage ->
            Log.error errorMessage
            false

    | None ->
        Log.error $"No renderer config found for layout %s{pageContext.Layout}"
        false
