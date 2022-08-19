#r "./src/FSharp.Static/bin/Debug/net6.0/FSharp.Static.Core.dll"
#load "../.paket/load/main.group.fsx"
#load "layout.fsx"

open Feliz.ViewEngine
open type Feliz.ViewEngine.Html

let generate (ctx: SiteContents) (projectRoot: ProjectRoot.T) (page: string) =
    let content = strong [ prop.text "Hello, world! index" ]

    Layout.mainPage ctx content
