using System;
using System.Collections.Generic;
using System.Linq;

namespace Grynwald.ChangeLog.Templates.ViewModel
{
    internal class ChangeLogEntryGroupViewModel
    {
        public string Title { get; }

        public IReadOnlyList<ChangeLogEntryViewModel> Entries { get; }


        public ChangeLogEntryGroupViewModel(string title, IEnumerable<ChangeLogEntryViewModel> entries)
        {
            if (String.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Value must not be null or whitespace", nameof(title));

            Title = title;
            Entries = entries.OrderBy(x => x.Date).ToList();
        }
    }
}
