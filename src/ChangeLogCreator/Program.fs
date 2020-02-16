open System
open System.IO
open CommandLine
open ChangeLogCreator

type Options = {
    [<Option('r', "repository")>] Repository: string;
    [<Option('o', "outputpath")>] OutputPath: string;
}

[<EntryPoint>]
let main argv =

    use commandLineParser = new Parser(fun opts -> 
        opts.CaseInsensitiveEnumValues <- true
        opts.CaseSensitive <- false
        opts.HelpWriter <- Console.Out)
    
    let validateOptions opts =
        if String.IsNullOrEmpty opts.Repository then 
            false 
        else
            Directory.Exists opts.Repository 
          
    let parserResult = commandLineParser.ParseArguments<Options>(argv)
  
    let run opts = 
        printfn "Loading commits from repository '%s'" opts.Repository

        let commitLoader (fromVersion:VersionInfo option) (toVersion:VersionInfo) = 
            let fromCommit = match fromVersion with
                                | Some versionInfo -> Some versionInfo.Tag.CommitId
                                | None -> None
            Git.getCommits opts.Repository fromCommit toVersion.Tag.CommitId 

        let document = Git.getTags opts.Repository 
                        |> ChangeLogBuilder.getVersions
                        |> ChangeLogBuilder.getChangeLog commitLoader
                        |> MarkdownRenderer.renderChangeLog 
        document.Save(opts.OutputPath, MarkdownRenderer.serializationOptions)
        0

    match parserResult with
        | :? Parsed<Options> as opts when (validateOptions opts.Value) ->  run opts.Value
        | :? Parsed<Options>  
        | :? NotParsed<Options> -> printfn "Invalid arguments"; 1
        | _ -> raise (InvalidOperationException "")
