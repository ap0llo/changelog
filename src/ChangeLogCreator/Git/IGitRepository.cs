using System;
using System.Collections.Generic;

namespace ChangeLogCreator.Git
{
    internal interface IGitRepository : IDisposable
    {
        IReadOnlyList<GitTag> GetTags();

        IReadOnlyList<GitCommit> GetCommits(string? fromCommit, string toCommit);
    }
}
