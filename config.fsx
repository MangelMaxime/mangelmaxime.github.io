#r "C:\\Users\\mange\\Workspaces\\Github\\ionide\\Fornax\\src\\Fornax\\bin\\Debug\\net5.0\\Fornax.Core.dll"

open Config
open System.IO

let postPredicate (projectRoot: string, page: string) =
    let fileName = Path.Combine(projectRoot, page)
    let ext = Path.GetExtension page

    if ext = ".md" then
        let ctn = File.ReadAllText fileName

        page.Contains("_public") |> not && ctn.Contains("layout: post")
    else
        false

let staticPredicate (projectRoot: string, page: string) =
    let ext = Path.GetExtension page

    let fileShouldBeExcluded =
        ext = ".fsx"
        || ext = ".md"
        || page.StartsWith "_public"
        || page.StartsWith "node_modules"
        || page.StartsWith "package.json"
        || page.StartsWith "package-lock.json"
        || page.StartsWith ".pollenrc.js"
        || page.StartsWith ".postcssrc.js"
        || page.StartsWith "_bin"
        || page.StartsWith "_lib"
        || page.StartsWith "_data"
        || page.StartsWith "_settings"
        || page.StartsWith "_config.yml"
        || page.StartsWith ".sass-cache"
        || page.StartsWith ".git"
        || page.StartsWith ".ionide"
        || page.StartsWith ".paket"
        || page.StartsWith ".vscode"
        || page.StartsWith ".config"
        || page.StartsWith ".editorconfig"
        || page.StartsWith ".fantomasignore"
        || page.StartsWith "paket-files"
        || page.StartsWith "style"
        || page.StartsWith "paket.lock"
        || page.StartsWith "paket.dependencies"
        || page.StartsWith "utils"
        || page.StartsWith "dart-sass"

    fileShouldBeExcluded |> not

let config = {
    Generators =
        [
            {
                Script = "sass.fsx"
                Trigger = OnFile "style/index.scss"
                OutputFile = NewFileName "style.css"
            }
            {
                Script = "post.fsx"
                Trigger = OnFilePredicate postPredicate
                OutputFile = ChangeExtension "html"
            }
            {
                Script = "staticfile.fsx"
                Trigger = OnFilePredicate staticPredicate
                OutputFile = SameFileName
            }
            {
                Script = "index.fsx"
                Trigger = Once
                OutputFile = NewFileName "index.html"
            }
        ]
}
