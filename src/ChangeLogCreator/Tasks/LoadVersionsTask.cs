using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ChangeLogCreator.Configuration;
using ChangeLogCreator.Git;
using ChangeLogCreator.Model;
using NuGet.Versioning;

namespace ChangeLogCreator.Tasks
{
    internal sealed class LoadVersionsTask : IChangeLogTask
    {
        private const RegexOptions s_RegexOptions = RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled;

        private readonly ChangeLogConfiguration m_Configuration;
        private readonly IGitRepository m_Repository;
        private readonly IReadOnlyList<Regex> m_TagPatterns;


        public LoadVersionsTask(ChangeLogConfiguration configuration, IGitRepository repository)
        {
            m_Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            m_Repository = repository ?? throw new ArgumentNullException(nameof(repository));

            m_TagPatterns = m_Configuration.TagPatterns.Select(x => new Regex(x, s_RegexOptions)).ToArray();
        }


        public Task RunAsync(ChangeLog changeLog)
        {
            foreach (var version in GetVersions())
            {
                var versionChangeLog = new SingleVersionChangeLog(version);
                changeLog.Add(versionChangeLog);
            }

            return Task.CompletedTask;
        }


        private IEnumerable<VersionInfo> GetVersions()
        {
            foreach (var tag in m_Repository.GetTags())
            {
                if(TryParseTagName(tag.Name, out var version))
                    yield return new VersionInfo(version, tag.Commit);                    
            }
        }


        private bool TryParseTagName(string tagName, [NotNullWhen(true)]out SemanticVersion? version)
        {
            version = default;
            foreach(var pattern in m_TagPatterns)
            {
                var match = pattern.Match(tagName);

                if (!match.Success)
                    continue;

                var versionString = match.Groups["version"]?.Value;
                if (!String.IsNullOrEmpty(versionString) && SemanticVersion.TryParse(versionString, out version))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
