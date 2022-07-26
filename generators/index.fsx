#r "../_lib/Fornax.Core.dll"
#load "../.paket/load/main.group.fsx"
#load "layout.fsx"

open Giraffe.ViewEngine

let generate (ctx: SiteContents) (projectRoot: string) (page: string) =
    let content = strong [] [ str "Hello, world! index" ]

    Layout.mainPage ctx content
