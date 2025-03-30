using System;
using System.Collections.Generic;
using System.Linq;
using Grynwald.ChangeLog.Git;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Pipeline;
using Microsoft.Extensions.Logging;
using NuGet.Versioning;

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

            // For each version, determine all reachable commits
            var commitsByVersion = new Dictionary<NuGetVersion, HashSet<GitCommit>>();
            foreach (var versionInfo in changeLog.Versions)
            {
                m_Logger.LogDebug($"Getting all commits reachable from version '{versionInfo.Version}'");
                var commits = m_Repository.GetCommits(null, versionInfo.Commit);
                commitsByVersion.Add(versionInfo.Version, commits.ToHashSet());
            }

            var sortedVersions = changeLog.Versions
                .OrderBy(x => x.Version)
                .ToArray();

            // For each version, remove all commits reachable by previous version
            for (var i = 0; i < sortedVersions.Length; i++)
            {
                var currentVersionInfo = sortedVersions[i];
                m_Logger.LogDebug($"Determining commits exclusive to version {currentVersionInfo.Version}");
                var currentVersionCommits = commitsByVersion[currentVersionInfo.Version].ToHashSet();

                for (var j = 0; j < i; j++)
                {
                    var previousVersionInfo = sortedVersions[j];
                    currentVersionCommits.ExceptWith(commitsByVersion[previousVersionInfo.Version]);
                }

                m_Logger.LogDebug($"Adding {currentVersionCommits.Count} commit to version {currentVersionInfo.Version}");
                foreach (var commit in currentVersionCommits)
                {
                    m_Logger.LogDebug($"Adding commit {commit.Id} to version {currentVersionInfo.Version}");
                    changeLog[currentVersionInfo].Add(commit);
                }
            }

            return ChangeLogTaskResult.Success;
        }
    }
}
