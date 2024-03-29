﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.ConventionalCommits;
using Grynwald.ChangeLog.Git;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Pipeline;
using Microsoft.Extensions.Logging;

namespace Grynwald.ChangeLog.Tasks
{
    [AfterTask(typeof(LoadCommitsTask))]
    [BeforeTask(typeof(RenderTemplateTask))]
    internal sealed class ParseCommitsTask : SynchronousChangeLogTask
    {
        private readonly ILogger<ParseCommitsTask> m_Logger;
        private readonly ChangeLogConfiguration m_Configuration;


        public ParseCommitsTask(ILogger<ParseCommitsTask> logger, ChangeLogConfiguration configuration)
        {
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            m_Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }


        protected override ChangeLogTaskResult Run(ApplicationChangeLog changeLog)
        {
            if (!changeLog.Versions.Any())
            {
                m_Logger.LogWarning("Changelog is empty, skipping parsing of commit messages");
                return ChangeLogTaskResult.Skipped;
            }

            m_Logger.LogInformation("Parsing commit messages");

            foreach (var versionChangeLog in changeLog.ChangeLogs)
            {
                foreach (var commit in versionChangeLog.AllCommits)
                {
                    if (TryGetChangeLogEntry(commit, out var entry))
                    {
                        m_Logger.LogDebug($"Adding changelog entry for commit '{commit.Id}' to version '{versionChangeLog.Version.Version}'");
                        versionChangeLog.Add(entry);
                    }
                }
            }

            return ChangeLogTaskResult.Success;
        }


        private bool TryGetChangeLogEntry(GitCommit commit, [NotNullWhen(true)] out ChangeLogEntry? entry)
        {
            var strictMode = m_Configuration.Parser.Mode == ChangeLogConfiguration.ParserMode.Strict;
            try
            {
                var parsed = CommitMessageParser.Parse(commit.CommitMessage, strictMode);
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
