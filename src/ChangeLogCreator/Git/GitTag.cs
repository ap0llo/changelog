using System;
using System.Diagnostics.CodeAnalysis;

namespace ChangeLogCreator.Git
{
    public sealed class GitTag : IEquatable<GitTag>
    {
        public string Name { get; }

        public string CommitId { get; }


        public GitTag(string name, string commitId)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Value must not be null or whitespace", nameof(name));

            if (String.IsNullOrWhiteSpace(commitId))
                throw new ArgumentException("Value must not be null or whitespace", nameof(commitId));
            Name = name;
            CommitId = commitId;
        }


        public override int GetHashCode()
        {
            unchecked
            {
                var hash = StringComparer.Ordinal.GetHashCode(Name) * 397;
                hash ^= StringComparer.OrdinalIgnoreCase.GetHashCode(CommitId);
                return hash;
            }
        }

        public override bool Equals(object? obj) => Equals(obj as GitTag);

        public bool Equals([AllowNull] GitTag other) =>
            other != null &&
            StringComparer.Ordinal.Equals(Name, other.Name) &&
            StringComparer.OrdinalIgnoreCase.Equals(CommitId, other.CommitId);
    }
}
