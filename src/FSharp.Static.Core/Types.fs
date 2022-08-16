namespace FSharp.Static.Core

open System.ComponentModel.Design


type Error = {
    Path: string
    Message: string
}

type SiteContext() =
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
                ResizeArray [
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

type Hooks =
    | PostGeneration

type GeneratorOutput =
    /// <summary>
    /// Generates a file with the same name.
    /// </summary>
    | SameFileName
    /// <summary>
    /// Generates a file with the provided name
    /// <param name="newFileName">The new file name</param>
    /// </summary>
    | NewFileName of newFileName : string
    /// <summary>
    /// Generate a file with the same base name but with the extension changed
    /// <param name="newExtension">The new extension</param>
    /// </summary>
    | ChangeExtension of newExtension : string

type GeneratorTrigger =
    | OnFile of fileName : string
    | OnFileExtensions of extensions : string
    | OnLayout of layout : string

type GeneratorConfig =
    {
        Script : string
        Trigger : GeneratorTrigger
        OutputFile : GeneratorOutput
    }

type Config =
    {
        Generators : GeneratorConfig list
        Hooks : Hooks list
    }
