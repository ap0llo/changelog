using System;
using Grynwald.ChangeLog.Configuration;

namespace Grynwald.ChangeLog.Templates.ViewModel
{
    /// <summary>
    /// Represents the view of a breaking change
    /// </summary>
    internal class BreakingChangeViewModel
    {
        private readonly ChangeLogConfiguration m_Configuration;


        /// <summary>
        /// Gets the breaking change's description
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the change log entry this breaking change is included in
        /// </summary>
        public ChangeLogEntryViewModel Entry { get; }

        /// <summary>
        /// Gets whether this breaking change was indicated in the commit message header or using a "BREAKING CHANGE" footer
        /// </summary>
        public bool IsBreakingChangeFromHeader { get; }


        /// <summary>
        /// Initializes a new instance of <see cref="BreakingChangeViewModel"/>
        /// </summary>
        public BreakingChangeViewModel(ChangeLogConfiguration configuration, string description, ChangeLogEntryViewModel entry, bool isBreakingChangeFromHeader)
        {
            m_Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            if (String.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Value must not be null or whitespace", nameof(description));

            if (entry is null)
                throw new ArgumentNullException(nameof(entry));

            Description = description;
            Entry = entry;
            IsBreakingChangeFromHeader = isBreakingChangeFromHeader;
        }
    }
}
