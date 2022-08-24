﻿module Nacara.Commands.Build

open Nacara
open Nacara.Core
open Nacara.Evaluator
open System
open System.IO

module Yaml = Legivel.Serialization

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
let rec private tryProcessFileContent
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

let private extractFiles (context: Context) =
    let sourceFiles =
        Directory.GetFiles(
            AbsolutePath.toString context.SourcePath,
            "*.*",
            SearchOption.AllDirectories
        )

    sourceFiles
    |> Array.map (fun file ->
        let fullPath = AbsolutePath.create file

        let relativePath =
            Path.GetRelativePath(ProjectRoot.toString context.ProjectRoot, file)
            |> RelativePath.create

        let pageId =
            relativePath
            |> RelativePath.toString
            |> Path.GetFileNameWithoutExtension
            |> PageId.create

        let rawText =
            File.ReadAllText(AbsolutePath.toString fullPath)

        let lines =
            rawText.Replace("\r\n", "\n").Split("\n")

        let fileExtension =
            Path.GetExtension(AbsolutePath.toString fullPath)[1..]

        let templateConfigList =
            context.Config.Templates
            |> List.filter (fun templateConfig ->
                templateConfig.Extension = fileExtension
            )

        match tryProcessFileContent templateConfigList lines [] with
        | Ok fileInfo ->
            {
                AbsolutePath = fullPath
                RelativePath = relativePath
                PageId = pageId
                Layout = fileInfo.FrontMatterData.Layout
                RawText = rawText
                FrontMatter = fileInfo.FrontMatter
                Content = fileInfo.Content
                Title = fileInfo.FrontMatterData.Title
            }
            |> Ok

        | Error errors ->
            [
                "Error while processing file "
                + file
                + ":"
                + "\n"
                + String.concat "\n" errors
            ]
            |> String.concat "\n"
            |> Error

    )
    |> Array.partitionMap (fun page ->
        match page with
        | Ok page -> Choice1Of2 page
        | Error errorMessage -> Choice2Of2 errorMessage
    )

let execute () =
    let context = Shared.loadConfigOrExit ()

    // Clean artifacts from previous builds
    Directory.Delete(AbsolutePath.toString context.OutputPath, true)

    // Ensure that the output directory exists.
    context.OutputPath
    |> AbsolutePath.toString
    |> Directory.CreateDirectory
    |> ignore


    let (validPages, erroredPages) =
        extractFiles context


    // Some page are errored report the errors and stop.
    if erroredPages.Length > 0 then
        erroredPages |> Array.iter Log.error

        1
    else

        validPages
        |> Array.iter (fun pageContext ->
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
                    RendererEvaluator.tryEvaluate rendererScript context pageContext

                match pageResult with
                | Ok pageContent ->
                    match rendererConfig.OutputAction with
                    | ChangeExtension newExtension ->
                        let destination =
                            Path.Combine (
                                ProjectRoot.toString context.ProjectRoot,
                                context.Config.Directory.Output,
                                Path.ChangeExtension(
                                    PageId.toString pageContext.PageId,
                                    newExtension)
                                )
                            |> AbsolutePath.create

                        File.WriteAllText(
                            AbsolutePath.toString destination,
                            pageContent
                        )

                        Log.info $"Generated: %s{PageId.toString pageContext.PageId}"

                | Error errorMessage ->
                    Log.error errorMessage
                    Environment.Exit(1)

            | None ->
                Log.error $"No renderer config found for layout %s{pageContext.Layout}"
        )

        0
