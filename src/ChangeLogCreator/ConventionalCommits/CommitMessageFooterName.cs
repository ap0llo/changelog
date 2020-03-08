using System;

namespace ChangeLogCreator.ConventionalCommits
{
    /// <summary>
    /// Represents the name of a commit message footer.
    /// </summary>
    public struct CommitMessageFooterName : IEquatable<CommitMessageFooterName>
    {
        private const string s_BREAKINGCHANGE = "BREAKING CHANGE";

        public static readonly CommitMessageFooterName BreakingChange = new CommitMessageFooterName(s_BREAKINGCHANGE);


        public string Value { get; }


        public bool IsBreakingChange => Value == s_BREAKINGCHANGE || StringComparer.OrdinalIgnoreCase.Equals(Value, "breaking-change");


        public CommitMessageFooterName(string key)
        {
            if (String.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Value must not be null or whitespace", nameof(key));

            Value = key;
        }




        public override int GetHashCode()
        {
            if (Value == s_BREAKINGCHANGE)
            {
                return StringComparer.OrdinalIgnoreCase.GetHashCode("breaking-change");
            }

            return StringComparer.OrdinalIgnoreCase.GetHashCode(Value);
        }

        public override bool Equals(object? obj) => obj is CommitMessageFooterName other && Equals(other);

        public bool Equals(CommitMessageFooterName other)
        {
            return IsBreakingChange && other.IsBreakingChange ||
                   StringComparer.OrdinalIgnoreCase.Equals(Value, other.Value);
        }

        public override string ToString() => Value;

        public static bool operator ==(CommitMessageFooterName left, CommitMessageFooterName right) => left.Equals(right);

        public static bool operator !=(CommitMessageFooterName left, CommitMessageFooterName right) => !left.Equals(right);
    }
}


