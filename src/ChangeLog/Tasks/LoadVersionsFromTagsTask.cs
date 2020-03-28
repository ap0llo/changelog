using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.Git;
using Grynwald.ChangeLog.Model;
using Microsoft.Extensions.Logging;
using NuGet.Versioning;

namespace Grynwald.ChangeLog.Tasks
{
    /// <summary>
    /// Tasks that loads versions from git tags in a repository
    /// </summary>
    internal sealed class LoadVersionsFromTagsTask : SynchronousChangeLogTask
    {
        private const RegexOptions s_RegexOptions = RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled;

        private readonly ILogger<LoadVersionsFromTagsTask> m_Logger;
        private readonly ChangeLogConfiguration m_Configuration;
        private readonly IGitRepository m_Repository;
        private readonly IReadOnlyList<Regex> m_TagPatterns;


        public LoadVersionsFromTagsTask(ILogger<LoadVersionsFromTagsTask> logger, ChangeLogConfiguration configuration, IGitRepository repository)
        {
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            m_Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            m_Repository = repository ?? throw new ArgumentNullException(nameof(repository));

            m_TagPatterns = m_Configuration.TagPatterns.Select(x => new Regex(x, s_RegexOptions)).ToArray();
        }


        protected override ChangeLogTaskResult Run(ApplicationChangeLog changeLog)
        {
            if (m_TagPatterns.Count == 0)
            {
                m_Logger.LogWarning("No tag patterns configured, skipping loading of versions from tags.");
                return ChangeLogTaskResult.Skipped;
            }

            m_Logger.LogInformation("Loading versions from git tags");

            foreach (var versionInfo in GetVersions())
            {
                if (changeLog.ContainsVersion(versionInfo.Version))
                {
                    m_Logger.LogError($"Cannot add version '{versionInfo.Version}' from tags because the changelog already contains this version.");
                    return ChangeLogTaskResult.Error;
                }

                m_Logger.LogDebug($"Adding version '{versionInfo.Version}' to changelog");
                var versionChangeLog = new SingleVersionChangeLog(versionInfo);
                changeLog.Add(versionChangeLog);
            }

            return ChangeLogTaskResult.Success;
        }


        private IEnumerable<VersionInfo> GetVersions()
        {
            var versions = new HashSet<NuGetVersion>();

            foreach (var tag in m_Repository.GetTags())
            {
                m_Logger.LogDebug($"Processing tag '{tag.Name}'");

                if (TryParseTagName(tag.Name, out var version))
                {
                    if (versions.Add(version))
                    {
                        yield return new VersionInfo(version, tag.Commit);
                    }
                    else
                    {
                        m_Logger.LogWarning($"Duplicate version '{version.ToNormalizedString()}', ignoring tag '{tag.Name}' ({tag.Commit})");
                    }
                }
            }
        }

        private bool TryParseTagName(string tagName, [NotNullWhen(true)] out NuGetVersion? version)
        {
            version = default;
            foreach (var pattern in m_TagPatterns)
            {
                var match = pattern.Match(tagName);

                if (!match.Success)
                    continue;

                var versionString = match.Groups["version"]?.Value;
                if (!String.IsNullOrEmpty(versionString) && NuGetVersion.TryParse(versionString, out version))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
