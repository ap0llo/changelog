namespace ChangeLogCreator

open ChangeLogCreator.MessageParser

module ChangeLogBuilder =

    let getEntries commits =

        let getChangeType (commit: ConventionalCommit) =
            match commit.Type.ToLower() with
                | "feat" -> Feature
                | "fix" -> BugFix
                | "wip" -> WorkInProgress
                | _ -> Other commit.Type
        
        let tryParseMessage (commit:GitCommit) = 
            let parserResult = MessageParser.parse commit.Message
            match parserResult with
                | Failed err  -> 
                    printfn "Failed to parse commit %s: %s" commit.Id err
                    None
                | Parsed parsed -> Some { 
                    Date = commit.Date
                    Type = getChangeType parsed
                    Scope = parsed.Scope
                    Summary = parsed.Description
                    CommitId = commit.Id
                }

        commits 
            |> Seq.choose tryParseMessage
            |> Seq.where (fun e -> e.Type <> WorkInProgress)           

    let getChangeLog commits = 
        let entries = getEntries commits

        let getEntriesOfType changeType entries =
            entries |> Seq.where (fun e -> e.Type = changeType) |> List.ofSeq

        let features = getEntriesOfType Feature entries
        let bugfixes = getEntriesOfType BugFix entries
        let otherChanges = entries
                            |> Seq.except features 
                            |> Seq.except bugfixes 
                            |> List.ofSeq
                            |> List.groupBy (fun e ->
                                let (Other foo) = e.Type;
                                foo)

        { ChangeLog.Features = features;
          ChangeLog.BugFixes = bugfixes;
          ChangeLog.OtherChanges = otherChanges }
