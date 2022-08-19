#r "./../src/FSharp.Static/bin/Debug/net6.0/FSharp.Static.Core.dll"
#load "../.paket/load/net6.0/Docs/docs.group.fsx"

open FSharp.Static.Core
type Page = {
    title: string
    link: string
}

let loader (context: Context) =
    context.Add(
        {
            title = "Home"
            link = "/"
        }
    )
