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
    internal abstract class LoadMessageOverridesTask : SynchronousChangeLogTask
    {
        private readonly ILogger<LoadMessageOverridesTask> m_Logger;
        private readonly ChangeLogConfiguration m_Configuration;

        public LoadMessageOverridesTask(ILogger<LoadMessageOverridesTask> logger, ChangeLogConfiguration configuration)
        {
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            m_Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }


        protected override ChangeLogTaskResult Run(ApplicationChangeLog changelog)
        {
            if (!m_Configuration.MessageOverrides.Enabled)
            {
                return ChangeLogTaskResult.Skipped;
            }

            if (!changelog.Versions.Any())
            {
                return ChangeLogTaskResult.Skipped;
            }

            m_Logger.LogInformation("Checking for commit message overrides");

            foreach (var versionChangeLog in changelog.ChangeLogs)
            {
                foreach (var commit in versionChangeLog.AllCommits.ToArray())
                {
                    if (TryGetOverrideMessage(commit, out var overrideMessage))
                    {
                        var newCommit = commit.WithCommitMessage(overrideMessage);

                        versionChangeLog.Remove(commit);
                        versionChangeLog.Add(newCommit);
                    }
                }
            }

            return ChangeLogTaskResult.Success;
        }


        protected abstract bool TryGetOverrideMessage(GitCommit commit, [NotNullWhen(true)] out string? message);
    }
}
