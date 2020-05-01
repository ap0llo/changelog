using System;
using System.Linq;
using Grynwald.ChangeLog.ConventionalCommits;
using Grynwald.ChangeLog.Model;
using Microsoft.Extensions.Logging;

namespace Grynwald.ChangeLog.Tasks
{
    internal sealed class FilterEntriesTask : SynchronousChangeLogTask
    {
        private readonly ILogger<FilterEntriesTask> m_Logger;


        public FilterEntriesTask(ILogger<FilterEntriesTask> logger)
        {
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        protected override ChangeLogTaskResult Run(ApplicationChangeLog changelog)
        {
            m_Logger.LogInformation($"Filtering change log entries.");

            foreach (var versionChangeLog in changelog.ChangeLogs)
            {
                var entriesToRemove = versionChangeLog.AllEntries
                    .Where(x => x.Type != CommitType.Feature && x.Type != CommitType.BugFix && !x.ContainsBreakingChanges);

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
