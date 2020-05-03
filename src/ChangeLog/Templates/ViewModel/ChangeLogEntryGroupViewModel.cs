using System;
using System.Collections.Generic;
using Grynwald.ChangeLog.Model;

namespace Grynwald.ChangeLog.Templates.ViewModel
{
    internal class ChangeLogEntryGroupViewModel
    {
        private readonly ChangeLogEntryGroup m_Model;


        public string DisplayName { get; }

        public IEnumerable<ChangeLogEntry> Entries => m_Model.Entries;


        public ChangeLogEntryGroupViewModel(string displayName, ChangeLogEntryGroup model)
        {
            if (String.IsNullOrWhiteSpace(displayName))
                throw new ArgumentException("Value must not be null or whitespace", nameof(displayName));

            DisplayName = displayName;
            m_Model = model ?? throw new ArgumentNullException(nameof(model));
        }
    }

}
