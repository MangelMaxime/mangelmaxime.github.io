#r "./src/FSharp.Static/bin/Debug/net6.0/FSharp.Static.Core.dll"
// #load "./src/FSharp.Static.Core/Types.fs"

open System.IO
open FSharp.Static.Core

let config =
    {
        Directory =
            {
                Input = "docsrc"
                Output = "public"
                Generators = "generators"
                Loaders = "loaders"
            }
    }
