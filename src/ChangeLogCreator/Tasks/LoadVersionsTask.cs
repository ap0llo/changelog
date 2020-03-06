using System;
using System.Collections.Generic;
using ChangeLogCreator.Git;
using ChangeLogCreator.Model;
using NuGet.Versioning;

namespace ChangeLogCreator.Tasks
{
    internal sealed class LoadVersionsTask : IChangeLogTask
    {
        private readonly IGitRepository m_Repository;

        public LoadVersionsTask(IGitRepository repository)
        {
            m_Repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }


        public void Run(ChangeLog changeLog)
        {
            foreach (var version in GetVersions())
            {
                var versionChangeLog = new SingleVersionChangeLog(version);
                changeLog.Add(versionChangeLog);
            }
        }


        private IEnumerable<VersionInfo> GetVersions()
        {
            foreach (var tag in m_Repository.GetTags())
            {
                var tagName = tag.Name.TrimStart('v'); //TODO: Prefix should be configurable
                if (SemanticVersion.TryParse(tagName, out var version))
                    yield return new VersionInfo(version, tag.Commit);
            }
        }
    }
}
