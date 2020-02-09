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
                    Type = getChangeType parsed
                    Scope = parsed.Scope
                    CommitId = commit.Id
                    Date = commit.Date
                    RawCommit = commit
                    ParsedCommit = parsed 
                }

        let getPriority changeType =
            match changeType with
                | "fix" -> 0
                | "feat" -> 1
                | _ -> 2

        commits 
            |> Seq.choose tryParseMessage
            |> Seq.where (fun e -> e.Type <> WorkInProgress)
            |> Seq.sortBy (fun e -> (getPriority e.ParsedCommit.Type), e.ParsedCommit.Type)

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
