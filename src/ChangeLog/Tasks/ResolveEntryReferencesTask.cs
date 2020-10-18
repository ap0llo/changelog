using System;
using System.Collections.Generic;
using System.Linq;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Model.Text;
using Microsoft.Extensions.Logging;

namespace Grynwald.ChangeLog.Tasks
{
    internal sealed class ResolveEntryReferencesTask : SynchronousChangeLogTask
    {
        private readonly ILogger<ResolveEntryReferencesTask> m_Logger;


        public ResolveEntryReferencesTask(ILogger<ResolveEntryReferencesTask> logger)
        {
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        protected override ChangeLogTaskResult Run(ApplicationChangeLog changelog)
        {
            m_Logger.LogInformation("Converting commit references to entry references");

            var entriesByCommitId = changelog.SelectMany(x => x.AllEntries).ToDictionary(x => x.Commit);

            foreach (var (currentEntry, footer) in EnumerateFootersWithEntries(changelog))
            {
                if (footer.Value is CommitReferenceTextElement commitReference)
                {
                    if (commitReference.CommitId == currentEntry.Commit)
                    {
                        m_Logger.LogDebug($"Ignoring self-reference in entry '{currentEntry.Commit}'");
                        continue;
                    }

                    if (entriesByCommitId.TryGetValue(commitReference.CommitId, out var referencedEntry))
                    {
                        m_Logger.LogDebug($"Detected reference to entry '{referencedEntry.Commit}' from entry '{currentEntry}'");
                        footer.Value = new ChangeLogEntryReferenceTextElement(footer.Value.Text, referencedEntry);
                    }
                    else
                    {
                        m_Logger.LogDebug($"No entry for commit '{commitReference.CommitId}' found");
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
