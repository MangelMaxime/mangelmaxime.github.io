#r "../src/Nacara/bin/Debug/net6.0/Nacara.Core.dll"
#load "../.paket/load/net6.0/Docs/docs.group.fsx"

open Nacara.Core

type SiteInfo = {
    title: string
    description: string
    postPageSize: int
}

let loader (context: Context) =
    let siteInfo = {
        title = "Sample Fornax blog"
        description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit"
        postPageSize = 5
    }

    context.Add(siteInfo)
