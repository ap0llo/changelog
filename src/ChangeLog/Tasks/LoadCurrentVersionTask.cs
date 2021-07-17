using System;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.Git;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Pipeline;
using Microsoft.Extensions.Logging;
using NuGet.Versioning;

namespace Grynwald.ChangeLog.Tasks
{
    [BeforeTask(typeof(RenderTemplateTask))]
    internal sealed class LoadCurrentVersionTask : SynchronousChangeLogTask
    {
        private readonly ILogger<LoadCurrentVersionTask> m_Logger;
        private readonly ChangeLogConfiguration m_Configuration;
        private readonly IGitRepository m_Repository;


        public LoadCurrentVersionTask(ILogger<LoadCurrentVersionTask> logger, ChangeLogConfiguration configuration, IGitRepository repository)
        {
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            m_Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            m_Repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }


        /// <inheritdoc />
        protected override ChangeLogTaskResult Run(ApplicationChangeLog changeLog)
        {
            if (String.IsNullOrEmpty(m_Configuration.CurrentVersion))
                return ChangeLogTaskResult.Skipped;

            if (!NuGetVersion.TryParse(m_Configuration.CurrentVersion, out var version))
            {
                m_Logger.LogError($"Invalid 'currentVersion' setting: '{m_Configuration.CurrentVersion}' is not a valid version");
                return ChangeLogTaskResult.Error;
            }

            if (changeLog.ContainsVersion(version))
            {
                m_Logger.LogError($"Cannot add current version '{version}' because the changelog already contains this version.");
                return ChangeLogTaskResult.Error;
            }

            var head = m_Repository.Head;
            m_Logger.LogDebug($"Adding version '{version.ToNormalizedString()}' (commit '{head.Id}', current repository HEAD)");
            var versionInfo = new VersionInfo(version, head.Id);
            changeLog.Add(new SingleVersionChangeLog(versionInfo));
            return ChangeLogTaskResult.Success;
        }
    }
}
