using System;
using System.Linq;
using System.Threading.Tasks;
using ChangeLogCreator.Configuration;
using ChangeLogCreator.Model;
using Microsoft.Extensions.Logging;
using NuGet.Versioning;

namespace ChangeLogCreator.Tasks
{
    internal sealed class FilterVersionsTask : IChangeLogTask
    {
        private readonly ILogger<FilterVersionsTask> m_Logger;
        private readonly ChangeLogConfiguration m_Configuration;


        public FilterVersionsTask(ILogger<FilterVersionsTask> logger, ChangeLogConfiguration configuration)
        {
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            m_Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }


        public Task RunAsync(ChangeLog changeLog)
        {
            if (String.IsNullOrEmpty(m_Configuration.VersionRange))
                return Task.CompletedTask;

            if (!VersionRange.TryParse(m_Configuration.VersionRange, out var versionRange))
            {
                m_Logger.LogWarning($"Failed to parse version range '{m_Configuration.VersionRange}'");
                return Task.CompletedTask;
            }

            m_Logger.LogInformation($"Filtering changelog using version range '{versionRange}'");

            foreach (var versionChangeLog in changeLog.ChangeLogs.ToArray())
            {
                if (!versionRange.Satisfies(versionChangeLog.Version.Version, VersionComparison.Default))
                {
                    m_Logger.LogDebug($"Removing version '{versionChangeLog.Version.Version}' from changelog");
                    changeLog.Remove(versionChangeLog);
                }
            }

            return Task.CompletedTask;
        }
    }
}
