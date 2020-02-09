namespace ChangeLogCreator

open ChangeLogCreator.MessageParser

module ChangeLogBuilder =
    
    type getCommits = VersionInfo option -> VersionInfo -> GitCommit list

    /// Gets the changelog entries from the specified commits. Ignores commits which could not be parsed as conventional commit
    let private getEntries commits =
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
                    Body = parsed.Body
                }
        commits 
            |> Seq.choose tryParseMessage
            |> Seq.where (fun e -> e.Type <> WorkInProgress)

    /// Extracts the versions from the specified list of git tags
    let getVersions (tags: GitTag list) : VersionInfo list =
        let tryGetVersionInfo (tag: GitTag) : VersionInfo option =
            let versionString = tag.Name.TrimStart('v') //TODO: Prefix should be configurable
            match SemanticVersion.parse versionString with
                | Some version -> Some { Tag = tag; Version = version }
                | None -> 
                    printfn "Failed to parse version tag '%s'" tag.Name
                    None

        tags |> List.choose tryGetVersionInfo
  
    // Gets the changelog for the specified version
    let getChangeLogForVersion (commitLoader: getCommits) (allVersions: VersionInfo seq) (version:VersionInfo)  : VersionChangeLog =

        let previousVersion : VersionInfo option =
            let lowerVersions = allVersions 
                                    |> Seq.where (fun (v:VersionInfo) -> v.Version < version.Version)
                                    |> Seq.sortByDescending (fun v -> v)
                                    |> List.ofSeq
            match lowerVersions with 
                | head::_ -> Some head
                | _ -> None

        let commits = commitLoader previousVersion version
        let entries = getEntries commits

        let entriesOfType changeType = entries |> Seq.where (fun e -> e.Type = changeType) |> List.ofSeq

        let features = entriesOfType Feature
        let bugfixes = entriesOfType BugFix

        let otherChanges = entries
                            |> Seq.except features 
                            |> Seq.except bugfixes 
                            |> List.ofSeq
                            |> List.groupBy (fun e -> match e.Type with
                                                        | Other content -> content
                                                        | _ -> "")
                               
        { VersionChangeLog.VersionInfo = version
          VersionChangeLog.Features = features;
          VersionChangeLog.BugFixes = bugfixes
          VersionChangeLog.OtherChanges = otherChanges }

    // Gets the full changelog for all versions
    let getChangeLog (commitLoader:getCommits) (versions: VersionInfo seq) : ChangeLog = 
        let versionChangeLogs = 
            versions 
                |> Seq.map (getChangeLogForVersion commitLoader versions)
                |> List.ofSeq
        { ChangeLog.Versions = versionChangeLogs }
