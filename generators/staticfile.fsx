#r "./src/FSharp.Static/bin/Debug/net6.0/FSharp.Static.Core.dll"

open System.IO

let generate (ctx: SiteContents) (projectRoot: ProjectRoot.T) (page: string) =
    let inputPath = Path.Combine(projectRoot, page)
    File.ReadAllBytes inputPath
