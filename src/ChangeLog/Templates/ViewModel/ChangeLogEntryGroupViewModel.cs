using System;
using System.Collections.Generic;
using System.Linq;
using Grynwald.ChangeLog.Configuration;

namespace Grynwald.ChangeLog.Templates.ViewModel
{
    /// <summary>
    /// Represents a grouping of changes in the change log
    /// </summary>
    internal class ChangeLogEntryGroupViewModel
    {
        private readonly ChangeLogConfiguration m_Configuration;


        /// <summary>
        /// Gets the title of the change group, e.g. <c>New Features</c>
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Gets the change log entries that are part of this group
        /// </summary>
        public IReadOnlyList<ChangeLogEntryViewModel> Entries { get; }


        /// <summary>
        /// Initializes a new instance of <see cref="ChangeLogEntryGroupViewModel"/>
        /// </summary>
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
