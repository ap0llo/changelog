namespace ChangeLogCreator

open LibGit2Sharp

/// Functions for getting data from Git repositories
module Git = 
    
    let getCommits repoPath =
        let toGitAuthor (signature : LibGit2Sharp.Signature) =
            { Email = signature.Email 
              Name = signature.Name }

        let toGitCommit (repo: LibGit2Sharp.Repository) (commit: LibGit2Sharp.Commit) =
            let id = repo.ObjectDatabase.ShortenObjectId commit
            { Id = id
              Message = commit.Message
              Date = commit.Author.When.DateTime
              Author = toGitAuthor commit.Author
            }

        use repo = new Repository(repoPath)
        repo.Commits 
            |> Seq.map (toGitCommit repo)
            |> List.ofSeq



