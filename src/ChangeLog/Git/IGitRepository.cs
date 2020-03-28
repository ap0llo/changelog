using System;
using System.Collections.Generic;

namespace Grynwald.ChangeLog.Git
{
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
    }
}
