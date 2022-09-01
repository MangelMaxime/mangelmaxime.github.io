namespace Nacara.Core

open System.ComponentModel.Design
open Legivel.Attributes
open System.IO

type DirectoryConfig =
    {
        Source: string
        Output: string
        Loaders: string
    }

type CopyConfig =
    | File of string
    | Directory of string

type RenderOutputAction = ChangeExtension of string

type RenderConfig =
    {
        Layout: string
        OutputAction: RenderOutputAction
        Script: string
    }

type FrontMatterConfig =
    {
        StartDelimiter: string
        EndDelimiter: string
    }

type TemplateConfig =
    {
        Extension: string
        FrontMatter: FrontMatterConfig
    }

type Config =
    {
        Port: int
        Directory: DirectoryConfig
        Render: RenderConfig list
        Templates: TemplateConfig list
    }

module Path =

    let normalize (path: string) = path.Replace("\\", "/")

module AbsolutePath =

    type T = private AbsolutePath of string

    let create (value: string) = value |> Path.normalize |> AbsolutePath

    let toString (AbsolutePath absolutePath: T) = absolutePath

    let getDirectoryName (AbsolutePath absolutePath: T) =
        Path.GetDirectoryName(absolutePath)

    let getFileName (AbsolutePath absolutePath: T) =
        Path.GetFileName(absolutePath)

module RelativePath =

    type T = private RelativePath of string

    let create (value: string) = value |> Path.normalize |> RelativePath

    let toString (RelativePath relativePath: T) = relativePath

// module FileName =

//     type T = private FileName of string

//     let create (value: string) = FileName value

//     let toString (FileName fileName: T) = fileName

module ProjectRoot =

    type T = private ProjectRoot of string

    let create (value: string) = value |> Path.normalize |> ProjectRoot

    let toString (ProjectRoot projectRoot: T) = projectRoot

module PageId =

    type T = private PageId of string

    let create (value: string) = PageId value

    let toString (PageId pageId: T) = pageId

// Add the concept of virtual files?
// Virtual files are files that are not present on the filesystem
// and would allow user to "inject" pages from a loader.
// Example usager: API documentation generation.
// It would read information from an fsproj and then generate a bunch of
// virtuals file for the different API pages.
type PageContext =
    {
        RelativePath: RelativePath.T
        AbsolutePath: AbsolutePath.T
        PageId: PageId.T
        Layout: string
        RawText: string
        FrontMatter: string
        Content: string
    }

type PageFrontMatter =
    {
        [<YamlField("layout")>]
        Layout: string
    }

type Context
    (
        projectRoot: ProjectRoot.T,
        isWatch: bool,
        logError: string -> unit
    ) =
    let container = new ServiceContainer()

    member _.Add(value: 'T) =
        let key = typeof<ResizeArray<'T>>

        match container.GetService(key) with
        | :? ResizeArray<'T> as service -> service.Add(value)
        | _ ->
            container.AddService(
                key,
                ResizeArray
                    [
                        value
                    ]
            )

    member _.Replace(value: ResizeArray<'T>) =
        let key = typeof<ResizeArray<'T>>

        container.RemoveService(key)
        container.AddService(key, value)

    member _.GetValues<'T>() : seq<'T> =
        let key = typeof<ResizeArray<'T>>
        container.GetService(key) :?> seq<'T>

    member this.TryGetValues<'T>() =
        let key = typeof<ResizeArray<'T>>

        if isNull (container.GetService(key)) then
            None
        else
            Some(this.GetValues<'T>())

    member this.TryGetValue<'T>() =
        this.TryGetValues<'T>() |> Option.bind (Seq.tryHead)

    member _.LogError(msg: string) = logError msg

    member this.Config = this.TryGetValue<Config>() |> Option.get

    member _.ProjectRoot = projectRoot

    member _.IsWatch = isWatch

    member this.OutputPath =
        Path.Combine(
            ProjectRoot.toString projectRoot,
            this.Config.Directory.Output
        )
        |> AbsolutePath.create

    member _.ConfigPath =
        Path.Combine(
            ProjectRoot.toString projectRoot,
            "nacara.fsx"
        )
        |> AbsolutePath.create

    member this.SourcePath =
        Path.Combine(
            ProjectRoot.toString projectRoot,
            this.Config.Directory.Source
        )
        |> AbsolutePath.create

    member this.LoadersPath =
        Path.Combine(
            ProjectRoot.toString projectRoot,
            this.Config.Directory.Loaders
        )
        |> AbsolutePath.create

type DependencyWatchInfo =
    {
        DependencyPath : AbsolutePath.T
        RendererPath : AbsolutePath.T
    }
