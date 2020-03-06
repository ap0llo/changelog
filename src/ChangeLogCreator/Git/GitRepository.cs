using System;
using System.Collections.Generic;
using System.Linq;
using LibGit2Sharp;

namespace ChangeLogCreator.Git
{
    public sealed class GitRepository : IGitRepository
    {
        private readonly string m_RepositoyPath;
        private readonly Repository m_Repository;


        public GitRepository(string repositoyPath)
        {
            if (String.IsNullOrWhiteSpace(repositoyPath))
                throw new ArgumentException("Value must not be null or whitespace.", nameof(repositoyPath));

            m_RepositoyPath = repositoyPath;
            m_Repository = new Repository(m_RepositoyPath);
        }


        public IReadOnlyList<GitCommit> GetCommits(GitId? fromCommit, GitId toCommit)
        {
            // Set up commit filter
            var filter = new CommitFilter() { IncludeReachableFrom = toCommit .Id  };
            if (fromCommit.HasValue)
            {
                filter.ExcludeReachableFrom = fromCommit.Value.Id;

            }

            return m_Repository.Commits.QueryBy(filter)
                .Select(ToGitCommit)
                .ToList();
        }

        public IReadOnlyList<GitTag> GetTags() => m_Repository.Tags.Select(ToGitTag).ToList();

        public void Dispose() => m_Repository.Dispose();


        private GitCommit ToGitCommit(Commit commit)
        {
            return new GitCommit(
                id: ToGitId(commit),
                commitMessage: commit.Message,
                date: commit.Author.When.DateTime,
                author: ToGitAuthor(commit.Author)
            );
        }

        private GitId ToGitId(GitObject gitObject)
        {
            var sha = m_Repository.ObjectDatabase.ShortenObjectId(gitObject);
            return new GitId(sha);
        }

        private GitAuthor ToGitAuthor(Signature signature) => new GitAuthor(name: signature.Name, email: signature.Email);

        private GitTag ToGitTag(Tag tag) => new GitTag(tag.FriendlyName, ToGitId(tag.Target));
    }
}
