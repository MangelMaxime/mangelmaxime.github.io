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

module Icon =

    let svg = tag "svg"

    let path = tag "path"

    let _viewBox = attr "viewBox"

    let _strokeWidth = attr "stroke-width"

    let _fill = attr "fill"

    let _xmlns = attr "xmlns"

    let _d = attr "d"

    let _stroke = attr "stroke"

    let _strokeLinecap = attr "stroke-linecap"

    let _strokeLinejoin = attr "stroke-linejoin"


    let twitter =
        svg [
                _width "24"
                _height "24"
                _strokeWidth "1.5"
                _viewBox "0 0 24 24"
                _fill "none"
                _xmlns "http://www.w3.org/2000/svg"
            ] [
            path [
                     _d
                         "M23 3.01006C23 3.01006 20.9821 4.20217 19.86 4.54006C19.2577 3.84757 18.4573 3.35675 17.567 3.13398C16.6767 2.91122 15.7395 2.96725 14.8821 3.29451C14.0247 3.62177 13.2884 4.20446 12.773 4.96377C12.2575 5.72309 11.9877 6.62239 12 7.54006V8.54006C10.2426 8.58562 8.50127 8.19587 6.93101 7.4055C5.36074 6.61513 4.01032 5.44869 3 4.01006C3 4.01006 -1 13.0101 8 17.0101C5.94053 18.408 3.48716 19.109 1 19.0101C10 24.0101 21 19.0101 21 7.51006C20.9991 7.23151 20.9723 6.95365 20.92 6.68006C21.9406 5.67355 23 3.01006 23 3.01006Z"
                     _stroke "currentColor"
                     _strokeLinecap "round"
                     _strokeLinejoin "round"

                 ] []
        ]

    let github =

        svg [
                _width "24"
                _height "24"
                _strokeWidth "1.5"
                _viewBox "0 0 24 24"
                _fill "none"
                _xmlns "http://www.w3.org/2000/svg"
            ] [
            path [
                     _d
                         "M16 22.0268V19.1568C16.0375 18.68 15.9731 18.2006 15.811 17.7506C15.6489 17.3006 15.3929 16.8902 15.06 16.5468C18.2 16.1968 21.5 15.0068 21.5 9.54679C21.4997 8.15062 20.9627 6.80799 20 5.79679C20.4558 4.5753 20.4236 3.22514 19.91 2.02679C19.91 2.02679 18.73 1.67679 16 3.50679C13.708 2.88561 11.292 2.88561 8.99999 3.50679C6.26999 1.67679 5.08999 2.02679 5.08999 2.02679C4.57636 3.22514 4.54413 4.5753 4.99999 5.79679C4.03011 6.81549 3.49251 8.17026 3.49999 9.57679C3.49999 14.9968 6.79998 16.1868 9.93998 16.5768C9.61098 16.9168 9.35725 17.3222 9.19529 17.7667C9.03334 18.2112 8.96679 18.6849 8.99999 19.1568V22.0268"
                     _stroke "currentColor"
                     _strokeLinecap "round"
                     _strokeLinejoin "round"
                 ] []
            path [
                     _d "M9 20.0267C6 20.9999 3.5 20.0267 2 17.0267"
                     _stroke "currentColor"
                     _strokeLinecap "round"
                     _strokeLinejoin "round"
                 ] []
        ]




let private socialIcon (title: string) (icon: XmlNode) (url: string) =
    li [ _class "social-icon"; _title title ] [
        a [] [ icon ]
    ]

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
            link [ _rel "stylesheet"
                   _href "/style.css" ]
            title [] [ str titleText ]
        ]
        body [] [
            pageContent

            footer [ _class "site-footer container" ] [
                div [ _class "site-footer-body" ] [
                    div [ _class "credit" ] [
                        str "© 2022 Maxime"
                    ]
                    ul [ _class "social" ] [
                        socialIcon "Twitter account" Icon.twitter ""
                        socialIcon "Gitub repository" Icon.github ""
                    ]
                ]
            ]

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
