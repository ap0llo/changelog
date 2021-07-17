using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Grynwald.ChangeLog.Git;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Model.Text;
using Grynwald.ChangeLog.Pipeline;
using Microsoft.Extensions.Logging;

namespace Grynwald.ChangeLog.Tasks
{
    /// <summary>
    /// Detects footer values that are references to git commits.
    /// When a valid reference to a git commit is found, replaces the footer's value (see <see cref="ChangeLogEntryFooter.Value"/>) with a <see cref="CommitReferenceTextElement"/>.
    /// </summary>
    [AfterTask(typeof(ParseCommitsTask))]
    [BeforeTask(typeof(RenderTemplateTask))]
    internal sealed class ParseCommitReferencesTask : SynchronousChangeLogTask
    {
        private static readonly Regex s_ObjectIdRegex = new Regex(@"^[\dA-z]+$", RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private readonly ILogger<ParseCommitReferencesTask> m_Logger;
        private readonly IGitRepository m_Repository;


        public ParseCommitReferencesTask(ILogger<ParseCommitReferencesTask> logger, IGitRepository repository)
        {
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            m_Repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }


        protected override ChangeLogTaskResult Run(ApplicationChangeLog changelog)
        {
            m_Logger.LogInformation("Searching for references to git commits in change log entry footers");

            foreach (var (entry, footer) in EnumerateFootersWithEntries(changelog))
            {
                var footerValue = footer.Value.Text.Trim();

                if (s_ObjectIdRegex.IsMatch(footerValue) && m_Repository.TryGetCommit(footerValue) is GitCommit commit)
                {
                    if (footer.Value is PlainTextElement)
                    {
                        m_Logger.LogDebug($"Detected reference to git commit '{commit.Id}' in '{footer.Name}' footer of entry {entry.Commit}");
                        footer.Value = new CommitReferenceTextElement(footer.Value.Text, commit.Id);
                    }
                    else
                    {
                        m_Logger.LogWarning($"Detected reference to git commit '{commit.Id}' in '{footer.Name}' footer of entry {entry.Commit}, but no link was added because the footer's link is already set.");
                    }
                }
            }

            return ChangeLogTaskResult.Success;
        }


        private IEnumerable<(ChangeLogEntry, ChangeLogEntryFooter)> EnumerateFootersWithEntries(ApplicationChangeLog changelog)
        {
            foreach (var versionChangeLog in changelog)
            {
                foreach (var entry in versionChangeLog)
                {
                    foreach (var footer in entry.Footers)
                    {
                        yield return (entry, footer);
                    }
                }
            }
        }
    }
}
