using System;
using System.Linq;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.ConventionalCommits;
using Grynwald.ChangeLog.Model;
using Microsoft.Extensions.Logging;

namespace Grynwald.ChangeLog.Tasks
{
    /// <summary>
    /// Tasks that removed ignored entries from the change log based on their type
    /// </summary>
    /// <remarks>
    /// Removes all entries that are of a type not configured to be included in the change log.
    /// Entries that contain breaking changes are kept regardless of their type.
    /// </remarks>
    internal sealed class FilterEntriesTask : SynchronousChangeLogTask
    {
        private readonly ILogger<FilterEntriesTask> m_Logger;
        private readonly ChangeLogConfiguration m_Configuration;


        public FilterEntriesTask(ILogger<FilterEntriesTask> logger, ChangeLogConfiguration configuration)
        {
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            m_Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }


        protected override ChangeLogTaskResult Run(ApplicationChangeLog changelog)
        {
            m_Logger.LogInformation($"Filtering change log entries.");

            var types = m_Configuration.EntryTypes.Select(x => new CommitType(x.Type)).ToHashSet();

            foreach (var versionChangeLog in changelog.ChangeLogs)
            {
                var entriesToRemove = versionChangeLog.AllEntries
                    .Where(x => !types.Contains(x.Type) && !x.ContainsBreakingChanges);

                foreach (var entry in entriesToRemove)
                {
                    m_Logger.LogDebug($"Removing changelog entry '{entry.Summary}' from changelog");
                    versionChangeLog.Remove(entry);
                }
            }

            return ChangeLogTaskResult.Success;
        }
    }
}
