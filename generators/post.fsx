#r "/home/mmangel/Workspaces/GitHub/ionide/Fornax/src/Fornax/bin/Debug/net5.0/Fornax.Core.dll"
#load "layout.fsx"

// open Giraffe.ViewEngine
open type Feliz.ViewEngine.Html

let generate (ctx: SiteContents) (_projectRoot: string) (page: string) =

    let postOpt =
        ctx.TryGetValues<PostLoader.Post>()
        |> Option.defaultValue Seq.empty
        |> Seq.tryFind (fun post -> post.relativeFile = page)

    match postOpt with
    | None ->
        let error = {
            Path = page
            Message = $"Post %s{page} not found in the context"
            Phase = Generating
        }

        ctx.AddError error

        Layout.generationErrorPage ctx

    | Some post ->

        div [ rawText post.content ] |> Layout.mainPage ctx
