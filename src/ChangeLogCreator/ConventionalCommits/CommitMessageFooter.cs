using System;
using System.Diagnostics.CodeAnalysis;

namespace ChangeLogCreator.ConventionalCommits
{
    public sealed class CommitMessageFooter : IEquatable<CommitMessageFooter>
    {
        public string Type { get; }

        public string Description { get; }


        public CommitMessageFooter(string type, string description)
        {
            if (String.IsNullOrEmpty(type))
                throw new ArgumentException("Value must not be null or empty", nameof(type));

            if (String.IsNullOrEmpty(description))
                throw new ArgumentException("Value must not be null or empty", nameof(description));

            Type = type;
            Description = description;
        }


        public override int GetHashCode()
        {
            unchecked
            {
                var hash = StringComparer.OrdinalIgnoreCase.GetHashCode(Type) * 397;
                hash ^= StringComparer.Ordinal.GetHashCode(Description);
                return hash;
            }
        }

        public override bool Equals(object? obj) => Equals(obj as CommitMessageFooter);

        public bool Equals([AllowNull] CommitMessageFooter other) =>
            other != null &&
            StringComparer.OrdinalIgnoreCase.Equals(Type, other.Type) &&
            StringComparer.Ordinal.Equals(Description, other.Description);
    }
}
