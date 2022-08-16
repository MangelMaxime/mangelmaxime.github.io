#r "C:\\Users\\mange\\Workspaces\\Github\\ionide\\Fornax\\src\\Fornax\\bin\\Debug\\net5.0\\Fornax.Core.dll"

type Page = {
    title: string
    link: string
}

let loader (projectRoot: string) (siteContent: SiteContents) =
    siteContent.Add(
        {
            title = "Home"
            link = "/"
        }
    )

    siteContent
