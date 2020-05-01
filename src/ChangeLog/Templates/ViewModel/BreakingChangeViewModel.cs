using System;
using Grynwald.ChangeLog.Configuration;

namespace Grynwald.ChangeLog.Templates.ViewModel
{
    internal class BreakingChangeViewModel
    {
        private readonly ChangeLogConfiguration m_Configuration;


        public string Description { get; }

        public ChangeLogEntryViewModel Entry { get; }


        public BreakingChangeViewModel(ChangeLogConfiguration configuration, string description, ChangeLogEntryViewModel entry)
        {
            m_Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            if (String.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Value must not be null or whitespace", nameof(description));

            if (entry is null)
                throw new ArgumentNullException(nameof(entry));

            Description = description;
            Entry = entry;
        }
    }
}
