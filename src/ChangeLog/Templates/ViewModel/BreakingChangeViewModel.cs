using System;

namespace Grynwald.ChangeLog.Templates.ViewModel
{
    internal class BreakingChangeViewModel
    {
        public string Description { get; }

        public ChangeLogEntryViewModel Entry { get; }


        public BreakingChangeViewModel(string description, ChangeLogEntryViewModel entry)
        {
            if (String.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Value must not be null or whitespace", nameof(description));

            if (entry is null)
                throw new ArgumentNullException(nameof(entry));

            Description = description;
            Entry = entry;
        }
    }
}
