using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Grynwald.ChangeLog.ConventionalCommits;

namespace Grynwald.ChangeLog.Model
{
    public sealed class SingleVersionChangeLog : IEnumerable<ChangeLogEntry>
    {
        private readonly List<ChangeLogEntry> m_Entries = new List<ChangeLogEntry>();


        public VersionInfo Version { get; }

        //TODO: Should AllEntries just return the union of FeatureEntries, BUgFixEntrues and BreakingChanges?
        public IEnumerable<ChangeLogEntry> AllEntries => m_Entries.OrderBy(x => x.Date);

        //TODO: Remove
        public IEnumerable<ChangeLogEntry> FeatureEntries => AllEntries.Where(e => e.Type == CommitType.Feature);

        //TODO: Remove
        public IEnumerable<ChangeLogEntry> BugFixEntries => AllEntries.Where(e => e.Type == CommitType.BugFix);

        //TODO: Replace with "AllBreakingChanges" and "AdditionalBreakingChanges" ???
        public IEnumerable<ChangeLogEntry> BreakingChanges => AllEntries.Where(e => e.ContainsBreakingChanges);


        public SingleVersionChangeLog(VersionInfo version)
        {
            Version = version ?? throw new ArgumentNullException(nameof(version));
        }


        public void Add(ChangeLogEntry entry)
        {
            if (entry is null)
                throw new ArgumentNullException(nameof(entry));

            m_Entries.Add(entry);
        }

        public void Remove(ChangeLogEntry entry)
        {
            if (entry is null)
                throw new ArgumentNullException(nameof(entry));

            m_Entries.Remove(entry);
        }


        public IEnumerator<ChangeLogEntry> GetEnumerator() => AllEntries.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => AllEntries.GetEnumerator();
    }
}
