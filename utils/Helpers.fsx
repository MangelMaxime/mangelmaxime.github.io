open System.IO

module Path =

    let normalize (path: string) = path.Replace("\\", "/")

    let relativePath (rootDir: string) (fullFilePath: string) =
        let rootDir = normalize rootDir

        // Find the lenght of the rootDir
        let chopLength =
            if rootDir.EndsWith("/") then
                rootDir.Length
            else
                rootDir.Length + 1

        // Remove the rootDir from the fullFilePath
        let relativeDir =
            fullFilePath |> Path.GetDirectoryName |> (fun x -> x.[chopLength..])

        let fileName = Path.GetFileName fullFilePath

        Path.Combine(relativeDir, fileName)
        |> normalize
