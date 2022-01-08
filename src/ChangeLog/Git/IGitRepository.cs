using System;
using System.Collections.Generic;

namespace Grynwald.ChangeLog.Git
{
    /// <summary>
    /// Provides access to a local git repository.
    /// </summary>
    public interface IGitRepository : IDisposable
    {
        /// <summary>
        /// Gets the currently checked-out commit.
        /// </summary>
        GitCommit Head { get; }

        /// <summary>
        /// Gets the repository's remotes.
        /// </summary>
        IEnumerable<GitRemote> Remotes { get; }

        /// <summary>
        /// Gets all the repository's tags.
        /// </summary>
        /// <returns></returns>
        IReadOnlyList<GitTag> GetTags();

        /// <summary>
        /// Gets all changes between two commits. (equivalent to <c>git log</c>)
        /// </summary>
        /// <remarks>
        /// The <see cref="GetCommits"/> method is used to get all commits between two commits in the history.
        /// The result includes all commits reachable from <paramref name="toCommit"/>.
        /// Optionally, commits reachable from <paramref name="fromCommit"/> can be excluded.
        /// When <paramref name="fromCommit"/> is <c>null</c>, <see cref="GetCommits" /> is equivalent to <c>git log toCommit</c>.
        /// When <paramref name="fromCommit"/> is *not* <c>null</c>, , <see cref="GetCommits" /> is equivalent to <c>git log fromCommit..toCommit</c>.
        /// </remarks>
        /// <seealso href="https://git-scm.com/docs/git-log">git-log Documentation</seealso>
        /// <seealso href="https://git-scm.com/book/en/v2/Git-Tools-Revision-Selection">Git Revision Selection</seealso>
        IReadOnlyList<GitCommit> GetCommits(GitId? fromCommit, GitId toCommit);

        /// <summary>
        /// Attempts to find a commit with the specified id
        /// </summary>
        /// <param name="id">The id to search a commit for. Value can be an abbreviated commit SHA.</param>
        GitCommit? TryGetCommit(string id);

        /// <summary>
        /// Gets all git notes for the specified object
        /// </summary>
        /// <param name="id">The id of the object to get notes for.</param>
        /// <returns>Returns all notes for the specified object or an empty list if no notes exist for the object.</returns>
        IReadOnlyList<GitNote> GetNotes(GitId id);
    }
}
