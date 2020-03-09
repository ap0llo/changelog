﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using ChangeLogCreator.ConventionalCommits;
using ChangeLogCreator.Git;
using ChangeLogCreator.Model;
using Microsoft.Extensions.Logging;

namespace ChangeLogCreator.Tasks
{
    internal sealed class ParseCommitsTask : IChangeLogTask
    {
        private readonly ILogger<ParseCommitsTask> m_Logger;
        private readonly IGitRepository m_Repository;


        public ParseCommitsTask(ILogger<ParseCommitsTask> logger, IGitRepository repository)
        {
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            m_Repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }


        public Task RunAsync(ChangeLog changeLog)
        {
            m_Logger.LogInformation("Parsing commit messages");

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
                    {
                        m_Logger.LogDebug($"Adding changelog entry for commit '{commit.Id}' to version '{current.Version}'");
                        changeLog[current].Add(entry);
                    }
                }
            }

            return Task.CompletedTask;
        }


        private bool TryGetChangeLogEntry(GitCommit commit, [NotNullWhen(true)]out ChangeLogEntry? entry)
        {
            try
            {
                var parsed = CommitMessageParser.Parse(commit.CommitMessage);
                entry = ChangeLogEntry.FromCommitMessage(commit, parsed);
                return true;
            }
            catch (ParserException)
            {
                m_Logger.LogDebug($"Ignoring commit '{commit.Id}' because the commit message could not be parsed.");
                entry = default;
                return false;
            }
        }
    }
}
