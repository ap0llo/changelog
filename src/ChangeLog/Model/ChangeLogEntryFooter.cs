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


        /// <summary>
        /// Initializes a new instance of <see cref="ChangeLogEntryFooter"/>
        /// </summary>
        /// <param name="name">The name of the footer.</param>
        /// <param name="value">The footer's value.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is empty.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is <c>null</c> or whitespace.</exception>
        public ChangeLogEntryFooter(CommitMessageFooterName name, string value)
        {
            if (name.IsEmpty)
                throw new ArgumentException("Footer name must not be empty", nameof(name));

            if (String.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Value must not be null or whitespace", nameof(value));

            Name = name;
            Value = value;
        }


        public static ChangeLogEntryFooter FromCommitMessageFooter(CommitMessageFooter footer) => new ChangeLogEntryFooter(footer.Name, footer.Value);
    }
}
