#r "./src/Nacara/bin/Debug/net6.0/Nacara.Core.dll"
// #load "./src/FSharp.Static.Core/Types.fs"

open System.IO
open Nacara.Core

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

        template {
            extension "md"
            front_matter { delimiter "---" }
        }

        template {
            extension "fsx"

            front_matter {
                start_delimiter "(***"
                end_delimiter "***)"
            }
        }

    }
