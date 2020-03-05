using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ChangeLogCreator.ConventionalCommits;
using ChangeLogCreator.Git;
using ChangeLogCreator.Model;

namespace ChangeLogCreator.Tasks
{
    internal sealed class ParseCommitsTask : IChangeLogTask
    {
        private readonly IGitRepository m_Repository;


        public ParseCommitsTask(IGitRepository repository)
        {
            m_Repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }


        public void Run(ChangeLog changeLog)
        {
            var sortedVersions = changeLog.Versions
                .OrderByDescending(x => x.Version)
                .ToArray();

            for (var i = 0; i < sortedVersions.Length; i++)
            {
                var current = sortedVersions[i];
                var previous = i + 1 < sortedVersions.Length ? sortedVersions[i + 1] : null;

                var commits = m_Repository.GetCommits(previous?.Commit, current.Commit);

                foreach (var commit in commits)
                {
                    if (TryGetChangeLogEntry(commit, out var entry))
                        changeLog[current].Add(entry);
                }
            }
        }


        private bool TryGetChangeLogEntry(GitCommit commit, [NotNullWhen(true)]out ChangeLogEntry? entry)
        {
            try
            {
                var parsed = CommitMessageParser.Parse(commit.CommitMessage);
                entry = ToChangeLogEntry(commit, parsed);
                return true;
            }
            catch (ParserException)
            {
                // TODO: Log error
                entry = default;
                return false;
            }
        }

        private ChangeLogEntry ToChangeLogEntry(GitCommit commit, CommitMessage commitMessage)
        {
            return new ChangeLogEntry(
                date: commit.Date,
                type: commitMessage.Header.Type,
                scope: commitMessage.Header.Scope,
                summary: commitMessage.Header.Description,
                body: commitMessage.Body,
                commit: commit.Id
            );
        }
    }
}
