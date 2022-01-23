using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.Git;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Pipeline;
using Microsoft.Extensions.Logging;

namespace Grynwald.ChangeLog.Tasks
{
    [AfterTask(typeof(LoadCommitsTask))]
    [BeforeTask(typeof(ParseCommitsTask))]
    internal sealed class LoadMessageOverridesTask : SynchronousChangeLogTask
    {
        private readonly ILogger<LoadMessageOverridesTask> m_Logger;
        private readonly ChangeLogConfiguration m_Configuration;
        private readonly IGitRepository m_Repository;

        public LoadMessageOverridesTask(ILogger<LoadMessageOverridesTask> logger, ChangeLogConfiguration configuration, IGitRepository repository)
        {
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            m_Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            m_Repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }


        protected override ChangeLogTaskResult Run(ApplicationChangeLog changelog)
        {
            if (!changelog.Versions.Any())
            {
                return ChangeLogTaskResult.Skipped;
            }

            // TODO: Check here if message overrides are enabled?? Or handle that in the composition root?           

            m_Logger.LogInformation("Checking for commit message overrides");

            foreach (var versionChangeLog in changelog.ChangeLogs)
            {
                foreach (var commit in versionChangeLog.AllCommits.ToArray())
                {
                    if (TryGetOverrideMessage(commit, out var overrideMessage))
                    {
                        m_Logger.LogInformation($"Commit message for commit '{commit.Id}' was overridden through git-notes. Using message from git-notes instead of commit message.");

                        var newCommit = commit.WithCommitMessage(overrideMessage);

                        versionChangeLog.Remove(commit);
                        versionChangeLog.Add(newCommit);
                    }
                }
            }

            return ChangeLogTaskResult.Success;
        }


        private bool TryGetOverrideMessage(GitCommit commit, [NotNullWhen(true)] out string? message)
        {
            var notes = m_Repository
                .GetNotes(commit.Id)
                .Where(n => n.Namespace == m_Configuration.MessageOverrides.GitNotesNamespace);

            if (notes.Any())
            {
                message = notes.Single().Message;
                return true;
            }

            message = default;
            return false;
        }
    }
}
