using System;
using System.Collections.Generic;

namespace ChangeLogCreator.Git
{
    public interface IGitRepository : IDisposable
    {
        IEnumerable<GitRemote> Remotes { get; }

        IReadOnlyList<GitTag> GetTags();

        IReadOnlyList<GitCommit> GetCommits(GitId? fromCommit, GitId toCommit);
    }
}
