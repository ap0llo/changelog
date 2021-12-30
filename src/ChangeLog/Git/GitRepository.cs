using System;
using System.Collections.Generic;
using System.Linq;
using LibGit2Sharp;

namespace Grynwald.ChangeLog.Git
{
    public sealed class GitRepository : IGitRepository
    {
        private readonly string m_RepositoryPath;
        private readonly Repository m_Repository;

        /// <inheritdoc />
        public GitCommit Head => ToGitCommit(m_Repository.Head.Tip);

        /// <inheritdoc />
        public IEnumerable<GitRemote> Remotes => m_Repository.Network.Remotes.Select(ToGitRemote);


        /// <summary>
        /// Initializes a new instance of <see cref="GitRepository"/>
        /// </summary>
        /// <param name="repositoryPath">The local path of the git repository.</param>
        public GitRepository(string repositoryPath)
        {
            if (String.IsNullOrWhiteSpace(repositoryPath))
                throw new ArgumentException("Value must not be null or whitespace.", nameof(repositoryPath));

            m_RepositoryPath = repositoryPath;

            try
            {
                m_Repository = new Repository(m_RepositoryPath);
            }
            catch (LibGit2Sharp.RepositoryNotFoundException ex)
            {
                throw new RepositoryNotFoundException($"Directory '{repositoryPath}' is not a git repository", ex);
            }
        }


        /// <inheritdoc />
        public IReadOnlyList<GitCommit> GetCommits(GitId? fromCommit, GitId toCommit)
        {
            // Set up commit filter
            var filter = new CommitFilter() { IncludeReachableFrom = toCommit.Id };
            if (fromCommit.HasValue)
            {
                filter.ExcludeReachableFrom = fromCommit.Value.Id;
            }

            return m_Repository.Commits.QueryBy(filter)
                .Select(ToGitCommit)
                .ToList();
        }

        /// <inheritdoc />
        public IReadOnlyList<GitTag> GetTags() => m_Repository.Tags.Select(ToGitTag).ToList();

        /// <inheritdoc />
        public GitCommit? TryGetCommit(string id)
        {
            if (String.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Value must not be null or whitespace", nameof(id));

            var commit = m_Repository.Lookup<Commit>(id);
            return commit is null ? null : ToGitCommit(commit);
        }

        /// <inheritdoc />
        public IReadOnlyList<GitNote> GetNotes(GitId id)
        {
            var gitObject = m_Repository.Lookup<GitObject>(id.Id);

            if (gitObject is null)
                throw new GitObjectNotFoundException($"Git object '{id}' was not found. Cannot retrieve notes for object.");

            return m_Repository.Notes[gitObject.Id]
                .Select(ToGitNote)
                .ToList();
        }

        /// <inheritdoc />
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
            var abbreviatedId = m_Repository.ObjectDatabase.ShortenObjectId(gitObject);
            return new GitId(gitObject.Sha, abbreviatedId);
        }

        private GitId ToGitId(ObjectId objectId)
        {
            var gitObject = m_Repository.Lookup<GitObject>(objectId);
            return ToGitId(gitObject);
        }

        private GitAuthor ToGitAuthor(Signature signature) => new GitAuthor(name: signature.Name, email: signature.Email);

        private GitTag ToGitTag(Tag tag) => new GitTag(tag.FriendlyName, ToGitId(tag.Target));

        private GitRemote ToGitRemote(Remote remote) => new GitRemote(remote.Name, remote.Url);

        private GitNote ToGitNote(Note note) =>
            new GitNote(ToGitId(note.TargetObjectId), note.Namespace, note.Message);

    }
}
