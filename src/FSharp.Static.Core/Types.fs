namespace FSharp.Static.Core

open System.ComponentModel.Design


// type GeneratorConfig =
//     {
//         Script: string
//         Trigger: GeneratorTrigger
//         OutputFile: GeneratorOutput
//     }

type DirectoryConfig =
    {
        Input: string
        Output: string
        Generators: string
        Loaders: string
    }

type Config =
    {
        Directory: DirectoryConfig
    }


type Hooks = | PostGeneration

type GeneratorOutput =
    /// <summary>
    /// Generates a file with the same name.
    /// </summary>
    | SameFileName
    /// <summary>
    /// Generates a file with the provided name
    /// <param name="newFileName">The new file name</param>
    /// </summary>
    | NewFileName of newFileName: string
    /// <summary>
    /// Generate a file with the same base name but with the extension changed
    /// <param name="newExtension">The new extension</param>
    /// </summary>
    | ChangeExtension of newExtension: string

type GeneratorTrigger =
    | OnFile of fileName: string
    | OnFileExtensions of extensions: string
    | OnLayout of layout: string

// module Config =

//     let create =
//         {
//             Input = "docsrc"
//             Output = "public"
//         }

//     let intput (value : string) (config : Config) =
//         { config with Input = value }

//     let output (value : string) (config : Config) =
//         { config with Output = value }

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


type Error =
    {
        Path: string
        Message: string
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
