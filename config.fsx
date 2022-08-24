#r "./src/FSharp.Static/bin/Debug/net6.0/FSharp.Static.Core.dll"
// #load "./src/FSharp.Static.Core/Types.fs"
#load "./loaders/PostLoader.fsx"

open System.IO
open FSharp.Static.Core

let config =
    config {
        directory {
            source "docs"
            output "public"
            loaders "loaders"
        }

        render {
            layout "post"
            script "layouts/post.fsx"
            change_extension_to "html"
        }

        // render {
        //     once
        //     script "layouts/contact.fsx"
        // }

        // copy {
        //     directory "static"
        //     file "favicon.ico"
        // }

        // templates [
        //     template {
        //         extension "md"
        //         front_matter {
        //             delimiter "---"
        //         }
        //     }
        // ]

        {
            Extension = "md"
            FrontMatter = {
                StartDelimiter = "---"
                EndDelimiter = "---"
            }
        }

        // add_plugin {

        // }
    }
