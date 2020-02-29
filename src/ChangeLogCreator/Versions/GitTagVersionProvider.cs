using System;
using System.Collections.Generic;
using System.Linq;
using ChangeLogCreator.Git;
using NuGet.Versioning;

namespace ChangeLogCreator.Versions
{
    internal class GitTagVersionProvider : IVersionProvider
    {
        private Lazy<IReadOnlyList<VersionInfo>> m_Versions;
        private readonly IGitRepository m_Repository;

        
        public IReadOnlyList<VersionInfo> AllVersions => m_Versions.Value;


        public GitTagVersionProvider(IGitRepository repository)
        {
            m_Repository = repository ?? throw new ArgumentNullException(nameof(repository));
            m_Versions = new Lazy<IReadOnlyList<VersionInfo>>(() => LoadVersions().ToArray());
        }


        private IEnumerable<VersionInfo> LoadVersions()
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
