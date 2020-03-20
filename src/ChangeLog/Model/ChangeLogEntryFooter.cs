using System;
using Grynwald.ChangeLog.ConventionalCommits;

namespace Grynwald.ChangeLog.Model
{
    public sealed class ChangeLogEntryFooter
    {
        /// <summary>
        /// Gets the footer's name.
        /// </summary>
        public CommitMessageFooterName Name { get; }

        /// <summary>
        /// Gets the footers value.
        /// </summary>
        public string Value { get; }

        public Uri? WebUri { get; set; }


        public ChangeLogEntryFooter(CommitMessageFooterName name, string value)
        {
            if (String.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Value must not be null or whitespace", nameof(value));

            Name = name;
            Value = value;
        }


        public static ChangeLogEntryFooter FromCommitMessageFooter(CommitMessageFooter footer) => new ChangeLogEntryFooter(footer.Name, footer.Value);
    }
}
