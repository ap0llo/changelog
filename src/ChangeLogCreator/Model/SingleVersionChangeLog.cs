using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ChangeLogCreator.Model
{
    public sealed class SingleVersionChangeLog : IEnumerable<ChangeLogEntry>
    {
        private readonly List<ChangeLogEntry> m_Entries = new List<ChangeLogEntry>();


        public VersionInfo Version { get; }

        public IReadOnlyList<ChangeLogEntry> Entries => m_Entries;


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

        public IEnumerator<ChangeLogEntry> GetEnumerator() => m_Entries.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => m_Entries.GetEnumerator();
    }
}
