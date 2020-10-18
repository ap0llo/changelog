using System;
using Grynwald.ChangeLog.ConventionalCommits;
using Grynwald.ChangeLog.Model.Text;

namespace Grynwald.ChangeLog.Model
{
    public sealed class ChangeLogEntryFooter
    {
        private TextElement m_Value;


        /// <summary>
        /// Gets the footer's name.
        /// </summary>
        public CommitMessageFooterName Name { get; }

        /// <summary>
        /// Gets the footers value.
        /// </summary>
        public TextElement Value
        {
            get => m_Value;
            set
            {
                if (value is null)
                    throw new ArgumentException("Value must not be null or whitespace", nameof(value));

                if (String.IsNullOrWhiteSpace(value.Text))
                    throw new ArgumentException("Text must not be null or whitespace", nameof(value));

                m_Value = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ChangeLogEntryFooter"/>
        /// </summary>
        /// <param name="name">The name of the footer.</param>
        /// <param name="value">The footer's value.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is empty.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is <c>null</c> or whitespace.</exception>
        public ChangeLogEntryFooter(CommitMessageFooterName name, TextElement value)
        {
            if (name.IsEmpty)
                throw new ArgumentException("Footer name must not be empty", nameof(name));

            if (value is null)
                throw new ArgumentException("Value must not be null or whitespace", nameof(value));

            if (String.IsNullOrWhiteSpace(value.Text))
                throw new ArgumentException("Text must not be null or whitespace", nameof(value));

            Name = name;
            m_Value = value;
        }


        public static ChangeLogEntryFooter FromCommitMessageFooter(CommitMessageFooter footer) => new ChangeLogEntryFooter(footer.Name, new PlainTextElement(footer.Value));
    }
}
