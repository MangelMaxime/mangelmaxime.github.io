#r "/home/mmangel/Workspaces/GitHub/ionide/Fornax/src/Fornax/bin/Debug/net5.0/Fornax.Core.dll"
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
                                    prop.className "icon-button2"
                                    prop.children [
                                        // span [
                                        //     prop.className "iconify-inline"
                                        //     prop.custom ("data-icon", "akar-icons:circle")
                                        // ]
                                        rawText """
   <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 32 32" aria-hidden="true" class="icon-button__icon" aria-hidden="true" focusable="false">
      <path d="M32 12.408l-11.056-1.607-4.944-10.018-4.944 10.018-11.056 1.607 8 7.798-1.889 11.011 9.889-5.199 9.889 5.199-1.889-11.011 8-7.798z"></path>
    </svg>
                                        """
                                    ]
                                ]
                            ]

                            li [
                                a [
                                    prop.className "icon-button2"
                                    prop.children [
                                        // span [
                                        //     prop.className "iconify-inline"
                                        //     prop.custom ("data-icon", "simple-icons:twitter")
                                        // ]
                                        // img [
                                        //     prop.className "image"
                                        //     prop.src "/images/Orion_twitter.svg"
                                        // ]
                                        rawText """

<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 64 64" aria-labelledby="title"
aria-describedby="desc" role="img" xmlns:xlink="http://www.w3.org/1999/xlink">
  <title>Twitter</title>
  <desc>A solid styled icon from Orion Icon Library.</desc>
  <path data-name="layer1"
  d="M64 13.194a23.1 23.1 0 0 1-7.3 2.1 14.119 14.119 0 0 0 5.5-7.2c-1.9 1.2-6.1 2.9-8.2 2.9a13.782 13.782 0 0 0-9.6-4 13.187 13.187 0 0 0-13.2 13.2 13.576 13.576 0 0 0 .3 2.9c-9.9-.3-21.5-5.2-28-13.7a13.206 13.206 0 0 0 4 17.4c-1.5.2-4.4-.1-5.7-1.4-.1 4.6 2.1 10.7 10.2 12.9-1.6.8-4.3.6-5.5.4.4 3.9 5.9 9 11.8 9-2.1 2.4-9.3 6.9-18.3 5.5a39.825 39.825 0 0 0 20.7 5.8 36.8 36.8 0 0 0 37-38.6v-.5a22.861 22.861 0 0 0 6.3-6.7z"
  fill="#202020"></path>
</svg>

                                        """
                                    ]
                                ]
                            ]

                            li [
                                rawText """

<a href="https://twitter.com/5t3ph/" class="button button-icon3" aria-label="Twitter">
<svg class="button__icon3" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" width="24" height="24"><path fill="none" d="M0 0h24v24H0z"/><path d="M22.162 5.656a8.384 8.384 0 0 1-2.402.658A4.196 4.196 0 0 0 21.6 4c-.82.488-1.719.83-2.656 1.015a4.182 4.182 0 0 0-7.126 3.814 11.874 11.874 0 0 1-8.62-4.37 4.168 4.168 0 0 0-.566 2.103c0 1.45.738 2.731 1.86 3.481a4.168 4.168 0 0 1-1.894-.523v.052a4.185 4.185 0 0 0 3.355 4.101 4.21 4.21 0 0 1-1.89.072A4.185 4.185 0 0 0 7.97 16.65a8.394 8.394 0 0 1-6.191 1.732 11.83 11.83 0 0 0 6.41 1.88c7.693 0 11.9-6.373 11.9-11.9 0-.18-.005-.362-.013-.54a8.496 8.496 0 0 0 2.087-2.165z"/></svg>
</a>

                                """
                            ]

                            li [
                                rawText """

<a href="https://twitter.com/5t3ph/" class="button button-icon" aria-label="Twitter">
          <svg aria-hidden="true" focusable="false" width="24" height="24" fill="currentColor" class="button__icon"><use href="#icon-twitter"></use></svg>
        </a>

                                """
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

            rawText """

<svg aria-hidden="true" focusable="false" hidden=""><defs><symbol id="icon-twitter" viewBox="0 0 26 28"><path d="M25.312 6.375c-0.688 1-1.547 1.891-2.531 2.609 0.016 0.219 0.016 0.438 0.016 0.656 0 6.672-5.078 14.359-14.359 14.359-2.859 0-5.516-0.828-7.75-2.266 0.406 0.047 0.797 0.063 1.219 0.063 2.359 0 4.531-0.797 6.266-2.156-2.219-0.047-4.078-1.5-4.719-3.5 0.313 0.047 0.625 0.078 0.953 0.078 0.453 0 0.906-0.063 1.328-0.172-2.312-0.469-4.047-2.5-4.047-4.953v-0.063c0.672 0.375 1.453 0.609 2.281 0.641-1.359-0.906-2.25-2.453-2.25-4.203 0-0.938 0.25-1.797 0.688-2.547 2.484 3.062 6.219 5.063 10.406 5.281-0.078-0.375-0.125-0.766-0.125-1.156 0-2.781 2.25-5.047 5.047-5.047 1.453 0 2.766 0.609 3.687 1.594 1.141-0.219 2.234-0.641 3.203-1.219-0.375 1.172-1.172 2.156-2.219 2.781 1.016-0.109 2-0.391 2.906-0.781z"></path> </symbol><symbol id="icon-buymeacoffee" viewBox="0 0 24 36"><g fill="none" fill-rule="evenodd"><path fill="#FF9100" d="M11.835 7.655l-8.74-.052 4.292 27.113h10.456l4.292-27.113z"></path><path fill="#FD0" d="M11.835 7.655l-8.74-.052 4.292 27.113h8.272L19.95 7.603z"></path><path fill="#FFF" d="M.599 7.603h22.55v-2.5H.6z"></path><path stroke="#000" stroke-width="1.17" d="M.599 7.603h22.55v-2.5H.6z"></path><path fill="#FFF" d="M18.78 1.04H4.813l-1.64 3.75H20.419z"></path><g stroke-width="1.17"><path stroke="#050505" d="M18.78 1.04H4.813l-1.64 3.75H20.419z"></path><path stroke="#000" d="M11.835 7.655l-10.3-.052 4.292 27.113h12.016l4.292-27.113z"></path></g><path fill="#FFF" d="M22.447 15.26H1.146l1.922 10.783 8.728-.094 8.728.094z"></path><path stroke="#000" stroke-width="1.17" d="M22.447 15.26H1.146l1.922 10.783 8.728-.094 8.728.094z"></path></g></symbol><symbol id="icon-github" viewBox="0 0 32 32"> <path d="M16 0.396c-8.84 0-16 7.164-16 16 0 7.071 4.584 13.067 10.94 15.18 0.8 0.151 1.093-0.344 1.093-0.769 0-0.38-0.013-1.387-0.020-2.72-4.451 0.965-5.389-2.147-5.389-2.147-0.728-1.847-1.78-2.34-1.78-2.34-1.449-0.992 0.112-0.972 0.112-0.972 1.607 0.112 2.451 1.648 2.451 1.648 1.427 2.447 3.745 1.74 4.66 1.331 0.144-1.035 0.556-1.74 1.013-2.14-3.553-0.4-7.288-1.776-7.288-7.907 0-1.747 0.62-3.173 1.647-4.293-0.18-0.404-0.72-2.031 0.14-4.235 0 0 1.34-0.429 4.4 1.64 1.28-0.356 2.64-0.532 4-0.54 1.36 0.008 2.72 0.184 4 0.54 3.040-2.069 4.38-1.64 4.38-1.64 0.86 2.204 0.32 3.831 0.16 4.235 1.020 1.12 1.64 2.547 1.64 4.293 0 6.147-3.74 7.5-7.3 7.893 0.56 0.48 1.080 1.461 1.080 2.96 0 2.141-0.020 3.861-0.020 4.381 0 0.42 0.28 0.92 1.1 0.76 6.401-2.099 10.981-8.099 10.981-15.159 0-8.836-7.164-16-16-16z"></path> </symbol> <symbol id="icon-codepen" viewBox="0 0 32 32"> <path d="M32 10.909l-0.024-0.116-0.023-0.067c-0.013-0.032-0.024-0.067-0.040-0.1-0.004-0.024-0.020-0.045-0.027-0.067l-0.047-0.089-0.040-0.067-0.059-0.080-0.061-0.060-0.080-0.060-0.061-0.040-0.080-0.059-0.059-0.053-0.020-0.027-14.607-9.772c-0.463-0.309-1.061-0.309-1.523 0l-14.805 9.883-0.051 0.053-0.067 0.075-0.049 0.060-0.067 0.080c-0.027 0.023-0.040 0.040-0.040 0.061l-0.067 0.080-0.027 0.080c-0.027 0.013-0.027 0.053-0.040 0.093l-0.013 0.067c-0.025 0.041-0.025 0.081-0.025 0.121v9.996c0 0.059 0.004 0.12 0.013 0.18l0.013 0.061c0.007 0.040 0.013 0.080 0.027 0.115l0.020 0.067c0.013 0.036 0.021 0.071 0.036 0.1l0.029 0.067c0 0.013 0.020 0.053 0.040 0.080l0.040 0.053c0.020 0.013 0.040 0.053 0.060 0.080l0.040 0.053 0.053 0.053c0.013 0.017 0.013 0.040 0.040 0.040l0.080 0.056 0.053 0.040 0.013 0.019 14.627 9.773c0.219 0.16 0.5 0.217 0.76 0.217s0.52-0.080 0.76-0.24l14.877-9.875 0.069-0.077 0.044-0.060 0.053-0.080 0.040-0.067 0.040-0.093 0.021-0.069 0.040-0.103 0.020-0.060 0.040-0.107v-10c0-0.067 0-0.127-0.021-0.187l-0.019-0.060 0.059 0.004zM16.013 19.283l-4.867-3.253 4.867-3.256 4.867 3.253-4.867 3.253zM14.635 10.384l-5.964 3.987-4.817-3.221 10.781-7.187v6.424zM6.195 16.028l-3.443 2.307v-4.601l3.443 2.301zM8.671 17.695l5.964 3.987v6.427l-10.781-7.188 4.824-3.223v-0.005zM17.387 21.681l5.965-3.973 4.817 3.227-10.783 7.187v-6.427zM25.827 16.041l3.444-2.293v4.608l-3.444-2.307zM23.353 14.388l-5.964-3.988v-6.44l10.78 7.187-4.816 3.224z"></path> </symbol> <symbol id="icon-devto" viewBox="0 0 512 512"> <path d="M188.437 221.898c-.009-18.484-11.717-46.436-46.525-46.436H95.379v162.505h45.573c36.093.06 47.496-27.934 47.496-46.408l-.011-69.661zm-27.735 69.479c0 5.705-1.907 9.978-5.716 12.826-3.81 2.846-7.632 4.271-11.443 4.271h-17.139V205.87h17.129c3.811 0 7.633 1.424 11.443 4.272 3.801 2.847 5.715 7.13 5.726 12.826v68.409z"></path> <path d="M287.328 204.485H235v37.734h31.987v29.041H235v37.724h52.339v29.044h-61.071c-10.959.282-20.074-8.379-20.349-19.339V195.813c-.267-10.951 8.406-20.045 19.357-20.319h62.063l-.011 28.991z"></path> <path d="M389.117 317.717c-12.965 30.201-36.191 24.189-46.594 0l-37.84-142.214h31.988l29.18 111.69 29.043-111.69h31.996l-37.773 142.214z"></path> <path fill="none" stroke="currentColor" stroke-linecap="round" stroke-linejoin="round" stroke-miterlimit="10" stroke-width="20" d="M424.401 471H87.599C61.863 471 41 450.137 41 424.401V87.599C41 61.863 61.863 41 87.599 41h336.802C450.137 41 471 61.863 471 87.599v336.802C471 450.137 450.137 471 424.401 471z"></path> </symbol> <symbol id="icon-youtube" viewBox="0 0 28 28"> <path d="M11.109 17.625l7.562-3.906-7.562-3.953v7.859zM14 4.156c5.891 0 9.797 0.281 9.797 0.281 0.547 0.063 1.75 0.063 2.812 1.188 0 0 0.859 0.844 1.109 2.781 0.297 2.266 0.281 4.531 0.281 4.531v2.125s0.016 2.266-0.281 4.531c-0.25 1.922-1.109 2.781-1.109 2.781-1.062 1.109-2.266 1.109-2.812 1.172 0 0-3.906 0.297-9.797 0.297v0c-7.281-0.063-9.516-0.281-9.516-0.281-0.625-0.109-2.031-0.078-3.094-1.188 0 0-0.859-0.859-1.109-2.781-0.297-2.266-0.281-4.531-0.281-4.531v-2.125s-0.016-2.266 0.281-4.531c0.25-1.937 1.109-2.781 1.109-2.781 1.062-1.125 2.266-1.125 2.812-1.188 0 0 3.906-0.281 9.797-0.281v0z"></path> </symbol></defs></svg>

            """

            // Add livereload script
            // match env with
            // | EnvLoader.Dev ->
            //     script [
            //         rawText livereloadCode
            //     ]
            // | EnvLoader.Prod -> ()

        ]
    ]
    |> Render.htmlDocument

let generationErrorPage (ctx: SiteContents) =
    div [
        h1 "Error"
        p "Something went wrong while generating the page."
    ]
    |> mainPage ctx
