using System;
using System.Diagnostics.CodeAnalysis;

namespace Grynwald.ChangeLog.ConventionalCommits
{
    /// <summary>
    /// Encapsulates information provided in a commit message footer
    /// </summary>
    /// <seealso href="https://www.conventionalcommits.org">Conventional Commits</see>
    /// <seealso cref="CommitMessage"/>
    public sealed class CommitMessageFooter : IEquatable<CommitMessageFooter>
    {
        /// <summary>
        /// Gets the footer's name.
        /// </summary>
        public CommitMessageFooterName Name { get; }

        /// <summary>
        /// Gets the footers value.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="CommitMessageFooter"/>.
        /// </summary>
        /// <param name="name">The footer's key (see <see cref="Name"/> property).</param>
        /// <param name="value">The footer's value (see <see cref="Value"/> property).</param>
        public CommitMessageFooter(CommitMessageFooterName name, string value)
        {
            if (String.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Value must not be null or whitespace", nameof(value));

            Name = name;
            Value = value;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hash = Name.GetHashCode() * 397;
                hash ^= StringComparer.Ordinal.GetHashCode(Value);
                return hash;
            }
        }

        /// <inheritdoc />
        public override bool Equals(object? obj) => Equals(obj as CommitMessageFooter);

        /// <inheritdoc />
        public bool Equals([AllowNull] CommitMessageFooter other) =>
            other != null &&
            Name.Equals(other.Name) &&
            StringComparer.Ordinal.Equals(Value, other.Value);
    }
}
