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

type RenderOutputAction =
    | ChangeExtension of string

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
        Extension : string
        FrontMatter: FrontMatterConfig
    }

type Config =
    {
        Directory: DirectoryConfig
        Render: RenderConfig list
        Templates : TemplateConfig list
    }

module AbsolutePath =

    type T = private AbsolutePath of string

    let create (value: string) = AbsolutePath value

    let toString (AbsolutePath absolutePath: T) = absolutePath

module RelativePath =

    type T = private RelativePath of string

    let create (value: string) = RelativePath value

    let toString (RelativePath relativePath: T) = relativePath

module FileName =

    type T = private FileName of string

    let create (value: string) = FileName value

    let toString (FileName fileName: T) = fileName

module ProjectRoot =

    type T = private ProjectRoot of string

    let create (value: string) = ProjectRoot value

    let toString (ProjectRoot projectRoot: T) = projectRoot

module PageId =

    type T = private PageId of string

    let create (value: string) = PageId value

    let toString (PageId pageId: T) = pageId

type Error =
    {
        Path: string
        Message: string
    }

type PageContext =
    {
        RelativePath : RelativePath.T
        AbsolutePath : AbsolutePath.T
        PageId : PageId.T
        Layout : string
        RawText : string
        FrontMatter : string
        Content : string
        Title : string option
    }

type PageFrontMatter =
    {
        [<YamlField("title")>]
        Title : string option
        [<YamlField("layout")>]
        Layout : string
    }

type Context(projectRoot : ProjectRoot.T, isWatch : bool, logError: string -> unit) =
    let container = new ServiceContainer()
    let errors = ResizeArray<Error>()

    member _.AddError(error: Error) = errors.Add(error)

    member _.Errors = errors

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

    member this.Config =
        this.TryGetValue<Config>()
        |> Option.get

    member _.ProjectRoot = projectRoot

    member _.IsWatch = isWatch

    member this.OutputPath =
        Path.Combine(
            ProjectRoot.toString projectRoot,
            this.Config.Directory.Output
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
