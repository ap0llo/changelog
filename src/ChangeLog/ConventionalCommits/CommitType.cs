using System;

namespace Grynwald.ChangeLog.ConventionalCommits
{
    /// <summary>
    /// Represents the type of a commit.
    /// </summary>
    public struct CommitType : IEquatable<CommitType>
    {
        public static readonly CommitType Feature = new CommitType("feat");
        public static readonly CommitType BugFix = new CommitType("fix");

        public string Type { get; }


        public CommitType(string type)
        {
            if (String.IsNullOrWhiteSpace(type))
                throw new ArgumentException("Value must not be null or whitespace", nameof(type));

            Type = type;
        }


        public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Type);

        public override bool Equals(object? obj) => obj is CommitType otherId && Equals(otherId);

        public bool Equals(CommitType other) => StringComparer.OrdinalIgnoreCase.Equals(Type, other.Type);

        public override string ToString() => Type;

        public static bool operator ==(CommitType left, CommitType right) => StringComparer.OrdinalIgnoreCase.Equals(left.Type, right.Type);

        public static bool operator !=(CommitType left, CommitType right) => !StringComparer.OrdinalIgnoreCase.Equals(left.Type, right.Type);
    }
}


