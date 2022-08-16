#r "C:\\Users\\mange\\Workspaces\\Github\\ionide\\Fornax\\src\\Fornax\\bin\\Debug\\net5.0\\Fornax.Core.dll"

type SiteInfo = {
    title: string
    description: string
    postPageSize: int
}

let loader (projectRoot: string) (siteContent: SiteContents) =
    let siteInfo = {
        title = "Sample Fornax blog"
        description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit"
        postPageSize = 5
    }

    siteContent.Add(siteInfo)

    siteContent
