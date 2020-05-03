using System;
using System.Collections.Generic;
using System.Linq;
using Grynwald.ChangeLog.ConventionalCommits;

namespace Grynwald.ChangeLog.Model
{
    /// <summary>
    /// Represents a grouping of change log entries based on the entry's type
    /// </summary>
    public sealed class ChangeLogEntryGroup
    {
        /// <summary>
        /// Gets the type of the entries
        /// </summary>
        public CommitType Type { get; }

        /// <summary>
        /// Gets the change log entries of the group
        /// </summary>
        public IReadOnlyList<ChangeLogEntry> Entries { get; }


        /// <summary>
        /// Initializes a new instance of <see cref="ChangeLogEntryGroup"/>
        /// </summary>
        public ChangeLogEntryGroup(CommitType type, IEnumerable<ChangeLogEntry> entries)
        {
            if (entries is null)
                throw new ArgumentNullException(nameof(entries));

            Type = type;
            Entries = entries.ToList();
        }

    }
}
