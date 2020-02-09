open System
open CommandLine;
open ChangeLogCreator;

type Options = {
    [<Option('r', "repository")>] Repository: string;
    [<Option('o', "outputpath")>] OutputPath: string;
}

[<EntryPoint>]
let main argv =

    use parser = new Parser(fun opts -> 
        opts.CaseInsensitiveEnumValues <- true
        opts.CaseSensitive <- false
        opts.HelpWriter <- Console.Out)
    
    let validateOptions opts =
        if String.IsNullOrEmpty opts.Repository then 
            false 
        else
            if IO.Directory.Exists opts.Repository then
                true
            else
                false
        
    let parserResult = parser.ParseArguments<Options>(argv)
  
    let run opts = 
        printfn "Loading commits from repository '%s'" opts.Repository

        Git.getCommits opts.Repository 
            |> ChangeLogBuilder.getChangeLog
            |> MarkdownRenderer.saveChangeLog opts.OutputPath
            |> ignore
        0

    match parserResult with
        | :? Parsed<Options> as opts when (validateOptions opts.Value) ->  run opts.Value
        | :? Parsed<Options>  
        | :? NotParsed<Options> -> printfn "Invalid arguments"; 1
        | _ -> raise (InvalidOperationException "")
