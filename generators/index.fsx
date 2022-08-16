#r "C:\\Users\\mange\\Workspaces\\Github\\ionide\\Fornax\\src\\Fornax\\bin\\Debug\\net5.0\\Fornax.Core.dll"
#load "../.paket/load/main.group.fsx"
#load "layout.fsx"

open Feliz.ViewEngine
open type Feliz.ViewEngine.Html

let generate (ctx: SiteContents) (projectRoot: string) (page: string) =
    let content = strong [ prop.text "Hello, world! index" ]

    Layout.mainPage ctx content
