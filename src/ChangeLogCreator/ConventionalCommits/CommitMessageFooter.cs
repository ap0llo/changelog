using System;
using System.Diagnostics.CodeAnalysis;

namespace ChangeLogCreator.ConventionalCommits
{
    /// <summary>
    /// Encapsulates information provided in a commit message footer
    /// </summary>
    /// <seealso href="https://www.conventionalcommits.org">Conventional Commits</see>
    /// <seealso cref="CommitMessage"/>
    public sealed class CommitMessageFooter : IEquatable<CommitMessageFooter>
    {
        /// <summary>
        /// Gets the footer's key.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Gets the footers value.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="CommitMessageFooter"/>.
        /// </summary>
        /// <param name="key">The footer's key (see <see cref="Key"/> property).</param>
        /// <param name="value">The footer's value (see <see cref="Value"/> property).</param>
        public CommitMessageFooter(string key, string value)
        {
            if (String.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Value must not be null or whitespace", nameof(key));

            if (String.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Value must not be null or whitespace", nameof(value));

            Key = key;
            Value = value;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hash = StringComparer.OrdinalIgnoreCase.GetHashCode(Key) * 397;
                hash ^= StringComparer.Ordinal.GetHashCode(Value);
                return hash;
            }
        }

        /// <inheritdoc />
        public override bool Equals(object? obj) => Equals(obj as CommitMessageFooter);

        /// <inheritdoc />
        public bool Equals([AllowNull] CommitMessageFooter other) =>
            other != null &&
            StringComparer.OrdinalIgnoreCase.Equals(Key, other.Key) &&
            StringComparer.Ordinal.Equals(Value, other.Value);
    }
}
