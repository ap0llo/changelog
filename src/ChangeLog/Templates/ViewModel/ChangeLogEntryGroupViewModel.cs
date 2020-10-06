using System;
using System.Collections.Generic;
using System.Linq;
using Grynwald.ChangeLog.Model;

namespace Grynwald.ChangeLog.Templates.ViewModel
{
    internal class ChangeLogEntryGroupViewModel
    {
        private readonly IReadOnlyList<ChangeLogEntry> m_Entries;


        public string DisplayName { get; }

        public IEnumerable<ChangeLogEntry> Entries => m_Entries;


        public ChangeLogEntryGroupViewModel(string displayName, IEnumerable<ChangeLogEntry> entries)
        {
            if (String.IsNullOrWhiteSpace(displayName))
                throw new ArgumentException("Value must not be null or whitespace", nameof(displayName));

            if (entries is null)
                throw new ArgumentNullException(nameof(entries));

            DisplayName = displayName;
            m_Entries = entries.ToArray();
        }
    }

}
