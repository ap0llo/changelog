using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.Git;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Pipeline;
using Microsoft.Extensions.Logging;

namespace Grynwald.ChangeLog.Tasks
{
    [AfterTask(typeof(LoadCommitsTask))]
    [BeforeTask(typeof(ParseCommitsTask))]
    internal sealed class LoadMessageOverridesFromFileSystemTask : LoadMessageOverridesTask
    {
        private class DuplicateOverrideMessageException : Exception
        { }


        private readonly ILogger<LoadMessageOverridesFromFileSystemTask> m_Logger;
        private readonly ChangeLogConfiguration m_Configuration;
        private readonly IGitRepository m_Repository;
        private Lazy<IReadOnlyDictionary<GitId, string>> m_OverrideMessages;


        public LoadMessageOverridesFromFileSystemTask(ILogger<LoadMessageOverridesFromFileSystemTask> logger, ChangeLogConfiguration configuration, IGitRepository repository) : base(logger)
        {
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            m_Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            m_Repository = repository ?? throw new ArgumentNullException(nameof(repository));
            m_OverrideMessages = new(LoadOverrideMessages);
        }


        protected override ChangeLogTaskResult Run(ApplicationChangeLog changelog)
        {
            try
            {
                return base.Run(changelog);
            }
            catch (DuplicateOverrideMessageException)
            {
                return ChangeLogTaskResult.Error;
            }
        }

        protected override bool TryGetOverrideMessage(GitCommit commit, [NotNullWhen(true)] out string? message)
        {
            if (m_OverrideMessages.Value.TryGetValue(commit.Id, out message))
            {
                m_Logger.LogInformation($"Commit message for commit '{commit.Id}' was overridden through filesystem. Using message from filesystem instead of commit message.");
                return true;
            }

            return false;
        }


        private IReadOnlyDictionary<GitId, string> LoadOverrideMessages()
        {
            var sourceDirectoryPath = Path.IsPathRooted(m_Configuration.MessageOverrides.SourceDirectoryPath)
                ? m_Configuration.MessageOverrides.SourceDirectoryPath
                : Path.Combine(m_Configuration.RepositoryPath, m_Configuration.MessageOverrides.SourceDirectoryPath);

            if (!Directory.Exists(sourceDirectoryPath))
            {
                m_Logger.LogDebug($"Commit message override directory '{sourceDirectoryPath}' does not exist, skipping loading of overrides.");
                return new Dictionary<GitId, string>();
            }

            m_Logger.LogDebug($"Loading commit message overrides from '{sourceDirectoryPath}'");
            var overrideMessages = new Dictionary<GitId, string>();
            foreach (var file in Directory.GetFiles(sourceDirectoryPath))
            {
                var name = Path.GetFileName(file);
                var commit = m_Repository.TryGetCommit(name);
                if (commit is null)
                {
                    m_Logger.LogWarning($"Ignoring file '{name}' from commit message override directory '{sourceDirectoryPath}' because no matching commit was found");
                    continue;
                }

                if (overrideMessages.ContainsKey(commit.Id))
                {
                    m_Logger.LogError($"Found multiple commit message override files for commit '{commit.Id}' in directory '{sourceDirectoryPath}'");
                    throw new DuplicateOverrideMessageException();
                }

                m_Logger.LogDebug($"Successfully loaded commit message override for commit '{commit.Id}'from file '{name}'");
                overrideMessages.Add(commit.Id, File.ReadAllText(file));
            }

            return overrideMessages;
        }
    }
}
