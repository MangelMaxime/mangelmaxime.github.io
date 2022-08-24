namespace FSharp.Static.Core

type ConfigBuilder() =

    member _.Yield(directory) =
        [
            fun (args: Config) ->
                { args with
                    Directory = directory
                }
        ]

    member _.Yield(renderConfig) =
        [
            fun (args: Config) ->
                { args with
                    Render = renderConfig :: args.Render
                }
        ]

    member _.Yield(renderConfig) =
        [
            fun (args: Config) ->
                { args with
                    Templates = renderConfig :: args.Templates
                }
        ]

    member _.Run(args) =
        let initialConfig =
            {
                Directory =
                    {
                        Source = "docsrc"
                        Output = "public"
                        Loaders = "loaders"
                    }
                Render = []
                Templates = []
            }

        List.fold (fun args f -> f args) initialConfig args

    member _.Combine(args) =
        let (a, b) = args

        List.concat
            [
                a
                b
            ]

    member this.For(args, delayedArgs) = this.Combine(args, delayedArgs ())

    member _.Delay f = f ()

    member _.Zero _ = ()


type DirectoryBuilder() =

    member _.Yield(_: unit) =
        {
            Source = "docs"
            Output = "public"
            Loaders = "loaders"
        }

    [<CustomOperation("source")>]
    member _.Source(directoryConfig, newValue: string) =
        { directoryConfig with
            Source = newValue
        }

    [<CustomOperation("output")>]
    member _.Output(directoryConfig, newValue: string) =
        { directoryConfig with
            Output = newValue
        }

    [<CustomOperation("loaders")>]
    member _.Loaders(directoryConfig, newValue: string) =
        { directoryConfig with
            Loaders = newValue
        }


type RenderBuilder() =
    member _.Yield(_: unit) =
        {
            Layout = ""
            OutputAction = ChangeExtension("html")
            Script = ""
        }

    member _.Run(renderConfig: RenderConfig) =
        printfn "Direcotry runned"
        renderConfig

    [<CustomOperation("layout")>]
    member _.Layout(renderConfig: RenderConfig, newValue: string) =
        { renderConfig with
            Layout = newValue
        }

    [<CustomOperation("change_extension_to")>]
    member _.ChangeExtension(renderConfig, newValue: string) =
        { renderConfig with
            OutputAction = ChangeExtension(newValue)
        }

    [<CustomOperation("script")>]
    member _.Script(renderConfig, newValue: string) =
        { renderConfig with
            Script = newValue
        }

// type TemplateBuilder() =

//     member _.Yield(renderConfig) =
//         [
//             fun (args: Config) ->
//                 { args with
//                     Render = renderConfig :: args.Render
//                 }
//         ]

//     member _.Run(args) =
//         let initialConfig =
//             {
//                 Extension = "md"
//                 FrontMatter =
//                     {
//                         StartDelimiter = "---"
//                         EndDelimiter = "---"
//                     }
//             }

//         List.fold (fun args f -> f args) initialConfig args

//     member _.Combine(args) =
//         let (a, b) = args

//         List.concat
//             [
//                 a
//                 b
//             ]

//     member this.For(args, delayedArgs) = this.Combine(args, delayedArgs ())


[<AutoOpen>]
module Builders =

    let config = ConfigBuilder()
    let directory = DirectoryBuilder()
    let render = RenderBuilder()
