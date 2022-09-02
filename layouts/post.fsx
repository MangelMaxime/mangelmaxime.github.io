#r "../src/Nacara/bin/Debug/net6.0/Nacara.Core.dll"
#load "./layout.fsx"
#load "../.paket/load/net6.0/Docs/docs.group.fsx"

// open Giraffe.ViewEngine
open type Feliz.ViewEngine.Html
// open Helpers
open Nacara.Core

let render (ctx: Context) (page: PageContext) =

    // div [ rawText (RelativePath.toString page.RelativePath) ] |> Layout.mainPage ctx
    "maxime page"
