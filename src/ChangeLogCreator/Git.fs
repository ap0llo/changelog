namespace ChangeLogCreator

open LibGit2Sharp

/// Functions for getting data from Git repositories
module Git = 
    
    let private shortenObjectId (repo: LibGit2Sharp.Repository) (gitObject: LibGit2Sharp.GitObject) = 
        repo.ObjectDatabase.ShortenObjectId gitObject

    /// Gets the commits in the specified range (equivalent to 'git log from..to')
    let getCommits (repoPath:string) (fromCommit:string option) (toCommit: string) : GitCommit list =
  
        let toGitAuthor (signature : LibGit2Sharp.Signature) =
            { Email = signature.Email 
              Name = signature.Name }

        let toGitCommit (repo: LibGit2Sharp.Repository) (commit: LibGit2Sharp.Commit) =            
            { Id = shortenObjectId repo commit
              Message = commit.Message
              Date = commit.Author.When.DateTime
              Author = toGitAuthor commit.Author
            }

        // Set up commit filter
        let filter = CommitFilter(IncludeReachableFrom = toCommit)          
        match fromCommit with
                | Some id -> filter.ExcludeReachableFrom <- id
                | None -> ()

        use repo = new Repository(repoPath)
        repo.Commits.QueryBy(filter)
            |> Seq.map (toGitCommit repo)
            |> List.ofSeq

    /// Gets all tags from the specified repository
    let getTags (repoPath:string) : GitTag list =        
        use repo = new Repository(repoPath)
        repo.Tags
             |> Seq.map (fun tag -> { Name = tag.FriendlyName; CommitId = shortenObjectId repo tag.Target })
             |> List.ofSeq
