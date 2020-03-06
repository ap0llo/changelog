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


        public string Key { get; }


        public bool IsBreakingChange => Key == s_BREAKINGCHANGE || StringComparer.OrdinalIgnoreCase.Equals(Key, "breaking-change");


        public CommitMessageFooterName(string key)
        {
            if (String.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Value must not be null or whitespace", nameof(key));

            Key = key;
        }




        public override int GetHashCode()
        {
            if (Key == s_BREAKINGCHANGE)
            {
                return StringComparer.OrdinalIgnoreCase.GetHashCode("breaking-change");
            }

            return StringComparer.OrdinalIgnoreCase.GetHashCode(Key);
        }

        public override bool Equals(object? obj) => obj is CommitMessageFooterName other && Equals(other);

        public bool Equals(CommitMessageFooterName other)
        {
            return IsBreakingChange && other.IsBreakingChange ||
                   StringComparer.OrdinalIgnoreCase.Equals(Key, other.Key);
        }

        public override string ToString() => Key;

        public static bool operator ==(CommitMessageFooterName left, CommitMessageFooterName right) => left.Equals(right);

        public static bool operator !=(CommitMessageFooterName left, CommitMessageFooterName right) => !left.Equals(right);
    }
}


