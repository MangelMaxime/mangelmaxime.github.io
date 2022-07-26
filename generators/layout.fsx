#r "../_lib/Fornax.Core.dll"
#if !FORNAX
#load "../loaders/PostLoader.fsx"
#load "../loaders/PageLoader.fsx"
#load "../loaders/GlobalLoader.fsx"
#load "../loaders/EnvLoader.fsx"
#load "../utils/Helpers.fsx"
#endif
#load "../.paket/load/main.group.fsx"

open Giraffe.ViewEngine

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

let mainPage (ctx: SiteContents) (pageContent: XmlNode) =
    let titleText =
        ctx.TryGetValue<GlobalLoader.SiteInfo>()
        |> Option.map (fun siteInfo -> siteInfo.title)
        |> Option.defaultValue ""

    let env =
        ctx.TryGetValue<EnvLoader.EnvInfo>()
        |> Option.map (fun envInfo -> envInfo.Env)
        |> Option.defaultValue EnvLoader.Dev

    html [] [
        head [] [
            meta [ _charset "utf-8" ]
            meta [ _name "viewport"
                   _content "width=device-width, initial-scale=1" ]
            title [] [ str titleText ]
        ]
        body [] [
            pageContent

            // Add livereload code
            match env with
            | EnvLoader.Dev ->
                script [ _type "text/javascript" ] [
                    rawText livereloadCode
                ]
            | EnvLoader.Prod -> ()
        ]
    ]
    |> RenderView.AsString.htmlDocument

let generationErrorPage (ctx: SiteContents) =
    div [] [
        h1 [] [ str "Error" ]
        p [] [
            str "Something went wrong while generating the page."
        ]
    ]
    |> mainPage ctx
