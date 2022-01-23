using System;
using System.Collections.Generic;
using System.Linq;
using Grynwald.ChangeLog.Git;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Pipeline;
using Microsoft.Extensions.Logging;

namespace Grynwald.ChangeLog.Tasks
{
    [AfterTask(typeof(LoadVersionsFromTagsTask))]
    [AfterTask(typeof(LoadCurrentVersionTask))]
    [BeforeTask(typeof(RenderTemplateTask))]
    internal sealed class LoadCommitsTask : SynchronousChangeLogTask
    {
        private readonly ILogger<LoadCommitsTask> m_Logger;
        private readonly IGitRepository m_Repository;


        public LoadCommitsTask(ILogger<LoadCommitsTask> logger, IGitRepository repository)
        {
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            m_Repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }


        protected override ChangeLogTaskResult Run(ApplicationChangeLog changeLog)
        {
            if (!changeLog.Versions.Any())
            {
                m_Logger.LogWarning("Changelog is empty, skipping loading of commit messages");
                return ChangeLogTaskResult.Skipped;
            }

            m_Logger.LogInformation("Loading commits");

            var sortedVersions = changeLog.Versions
                .OrderByDescending(x => x.Version)
                .ToArray();

            for (var i = 0; i < sortedVersions.Length; i++)
            {
                var current = sortedVersions[i];
                var previous = i + 1 < sortedVersions.Length ? sortedVersions[i + 1] : null;

                IReadOnlyList<GitCommit> commits;
                if (previous is null)
                {
                    m_Logger.LogDebug($"Adding all commits up to '{current.Commit.Id}' to version '{current.Version.Version}'");
                    commits = m_Repository.GetCommits(null, current.Commit);
                }
                else
                {
                    m_Logger.LogDebug($"Adding commits between '{previous.Commit.Id}' and '{current.Commit.Id}' to version '{current.Version.Version}'");
                    commits = m_Repository.GetCommits(previous.Commit, current.Commit);
                }

                foreach (var commit in commits)
                {
                    changeLog[current].Add(commit);
                }
            }

            return ChangeLogTaskResult.Success;
        }
    }
}
