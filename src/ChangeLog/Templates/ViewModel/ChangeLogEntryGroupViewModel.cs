using System;
using System.Collections.Generic;
using System.Linq;
using Grynwald.ChangeLog.Configuration;

namespace Grynwald.ChangeLog.Templates.ViewModel
{
    internal class ChangeLogEntryGroupViewModel
    {
        private readonly ChangeLogConfiguration m_Configuration;


        public string Title { get; }

        public IReadOnlyList<ChangeLogEntryViewModel> Entries { get; }


        public ChangeLogEntryGroupViewModel(ChangeLogConfiguration configuration, string title, IEnumerable<ChangeLogEntryViewModel> entries)
        {
            m_Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            if (String.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Value must not be null or whitespace", nameof(title));

            Title = title;
            Entries = entries.OrderBy(x => x.Date).ToList();
        }
    }
}
