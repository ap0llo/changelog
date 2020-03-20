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
    internal sealed class LoadVersionsTask : IChangeLogTask
    {
        private const RegexOptions s_RegexOptions = RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled;

        private readonly ILogger<LoadVersionsTask> m_Logger;
        private readonly ChangeLogConfiguration m_Configuration;
        private readonly IGitRepository m_Repository;
        private readonly IReadOnlyList<Regex> m_TagPatterns;


        public LoadVersionsTask(ILogger<LoadVersionsTask> logger, ChangeLogConfiguration configuration, IGitRepository repository)
        {
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            m_Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            m_Repository = repository ?? throw new ArgumentNullException(nameof(repository));

            m_TagPatterns = m_Configuration.TagPatterns.Select(x => new Regex(x, s_RegexOptions)).ToArray();
        }


        public Task RunAsync(ApplicationChangeLog changeLog)
        {
            m_Logger.LogInformation("Loading versions from git tags");

            foreach (var version in GetVersions())
            {
                m_Logger.LogDebug($"Adding version '{version.Version}' to changelog");
                var versionChangeLog = new SingleVersionChangeLog(version);
                changeLog.Add(versionChangeLog);
            }

            return Task.CompletedTask;
        }


        private IEnumerable<VersionInfo> GetVersions()
        {
            foreach (var tag in m_Repository.GetTags())
            {
                m_Logger.LogDebug($"Processing tag '{tag.Name}'");

                if (TryParseTagName(tag.Name, out var version))
                    yield return new VersionInfo(version, tag.Commit);
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
