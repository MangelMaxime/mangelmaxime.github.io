#r "/home/mmangel/Workspaces/Github/ionide/Fornax/src/Fornax/bin/Debug/net5.0/Fornax.Core.dll"
#if !FORNAX
#load "../loaders/PostLoader.fsx"
#load "../loaders/PageLoader.fsx"
#load "../loaders/GlobalLoader.fsx"
#load "../loaders/EnvLoader.fsx"
#load "../utils/Helpers.fsx"
#endif
#load "../.paket/load/main.group.fsx"

// open Giraffe.ViewEngine
open Feliz.ViewEngine
open type Feliz.ViewEngine.Html

let livereloadCode =
    """
var wsUri = "ws://localhost:8080/websocket";

function init() {
    websocket = new WebSocket(wsUri);
    websocket.onclose = function(evt) { onClose(evt) };
}

function onClose(evt) {
    console.log('closing');
    websocket.close();
    document.location.reload();
}

window.addEventListener("load", init, false);
</script>
        """


let mainPage (ctx: SiteContents) (pageContent: ReactElement) =
    let titleText =
        ctx.TryGetValue<GlobalLoader.SiteInfo>()
        |> Option.map (fun siteInfo -> siteInfo.title)
        |> Option.defaultValue ""

    let env =
        ctx.TryGetValue<EnvLoader.EnvInfo>()
        |> Option.map (fun envInfo -> envInfo.Env)
        |> Option.defaultValue EnvLoader.Dev

    html [
        head [
            meta [
                prop.charset "utf-8"
            ]
            meta [
                prop.name "viewport"
                prop.content "width=device-width, initial-scale=1"
            ]
            link [
                prop.rel "stylesheet"
                prop.href "/style.css"
            ]
            title titleText
            script [
                prop.src "https://code.iconify.design/2/2.2.1/iconify.min.js"
            ]
        ]

        body [
            div [
                prop.className "container"
                prop.children [
                    pageContent
                ]
            ]

            span [
                prop.className "iconify-inline"
                prop.custom ("data-icon", "simple-icons:github")
            ]

            footer [
                prop.className "site-footer"
                prop.children [
                    p [
                        prop.className "site-footer--copyright"
                        prop.text "© 2022 Mangel Maxime"
                    ]
                    ul [
                        prop.className "site-footer--social"
                        prop.role "list"
                        prop.children [

                            li [
                                a [
                                    prop.className "icon"
                                    prop.children [
                                        span [
                                            prop.className "iconify-inline"
                                            prop.custom ("data-icon", "simple-icons:github")
                                        ]
                                    ]
                                ]
                            ]

                            li [
                                a [
                                    prop.className "icon"
                                    prop.children [
                                        span [
                                            prop.className "iconify-inline"
                                            prop.custom ("data-icon", "simple-icons:twitter")
                                        ]
                                    ]
                                ]
                            ]
                        ]
                    ]
                    p [
                        prop.className "site-footer--support"
                        prop.children [
                            text "Support my projects - "
                            a [
                                prop.href "https://www.patreon.com/MangelMaxime"
                                prop.text "Patreon"
                            ]
                        ]
                    ]
                ]
            ]

            // Add livereload script
            match env with
            | EnvLoader.Dev ->
                script [
                    rawText livereloadCode
                ]
            | EnvLoader.Prod -> ()

        ]
    ]
    |> Render.htmlDocument

let generationErrorPage (ctx: SiteContents) =
    div [
        h1 "Error"
        p "Something went wrong while generating the page."
    ]
    |> mainPage ctx
