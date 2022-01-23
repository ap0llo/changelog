using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.Git;
using Grynwald.ChangeLog.Pipeline;
using Microsoft.Extensions.Logging;

namespace Grynwald.ChangeLog.Tasks
{
    [AfterTask(typeof(LoadCommitsTask))]
    [BeforeTask(typeof(ParseCommitsTask))]
    internal sealed class LoadMessageOverridesFromGitNotesTask : LoadMessageOverridesTask
    {
        private readonly ILogger<LoadMessageOverridesFromGitNotesTask> m_Logger;
        private readonly ChangeLogConfiguration m_Configuration;
        private readonly IGitRepository m_Repository;


        public LoadMessageOverridesFromGitNotesTask(ILogger<LoadMessageOverridesFromGitNotesTask> logger, ChangeLogConfiguration configuration, IGitRepository repository) : base(logger)
        {
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            m_Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            m_Repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }


        protected override bool TryGetOverrideMessage(GitCommit commit, [NotNullWhen(true)] out string? message)
        {
            var notes = m_Repository
                .GetNotes(commit.Id)
                .Where(n => n.Namespace == m_Configuration.MessageOverrides.GitNotesNamespace);

            if (notes.Any())
            {
                m_Logger.LogInformation($"Commit message for commit '{commit.Id}' was overridden through git-notes. Using message from git-notes instead of commit message.");
                message = notes.Single().Message;
                return true;
            }

            message = default;
            return false;
        }
    }
}
