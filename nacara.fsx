#r "./src/Nacara/bin/Debug/net6.0/Nacara.Core.dll"

open System.IO
open Nacara.Core

let config =
    config {
        port 8080

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
