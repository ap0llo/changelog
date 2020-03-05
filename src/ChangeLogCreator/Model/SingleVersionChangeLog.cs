using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ChangeLogCreator.ConventionalCommits;

namespace ChangeLogCreator.Model
{
    public sealed class SingleVersionChangeLog : IEnumerable<ChangeLogEntry>
    {
        private readonly List<ChangeLogEntry> m_Entries = new List<ChangeLogEntry>();


        public VersionInfo Version { get; }

        public IEnumerable<ChangeLogEntry> AllEntries => m_Entries.OrderBy(x => x.Date);

        public IEnumerable<ChangeLogEntry> FeatureEntries => AllEntries.Where(e => e.Type == CommitType.Feature);

        public IEnumerable<ChangeLogEntry> BugFixEntries => AllEntries.Where(e => e.Type == CommitType.BugFix);


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

        public IEnumerator<ChangeLogEntry> GetEnumerator() => AllEntries.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => AllEntries.GetEnumerator();
    }
}
