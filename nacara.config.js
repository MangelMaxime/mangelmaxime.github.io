// For more information about the config, please visit:
// https://mangelmaxime.github.io/Nacara/nacara/configuration.html
export default {
    "siteMetadata": {
        "url": "https://your-nacara-test-site.com",
        "baseUrl": "/",
        // Please change this to your repo
        "editUrl" : "https://github.com/MangelMaxime/Nacara/edit/master/docs",
        "title": "Mangel Maxime"
    },
    "navbar": {
        "start": [
            {
                "section": "documentation",
                "url": "/documentation/introduction.html",
                "label": "Documentation"
            }
        ],
        "end": [
            // Please change it your repo or delete the item if you don't need it
            {
                "url": "https://github.com/MangelMaxime/Nacara",
                "icon": "fab fa-github",
                "label": "Github"
            }
        ]
    },
    "remarkPlugins": [
        {
            "resolve": "gatsby-remark-vscode",
            "property": "remarkPlugin",
            "options": {
                "theme": "Atom One Light",
                "extensions": [
                    "vscode-theme-onelight"
                ]
            }
        }
    ],
    "layouts": [
        "nacara-layout-standard"
    ]
}
