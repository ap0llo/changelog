using System;
using System.Text.RegularExpressions;

namespace Grynwald.ChangeLog.Git
{
    /// <summary>
    /// Represents a git commit id
    /// </summary>
    public struct GitId : IEquatable<GitId>
    {
        private static readonly Regex s_ObjectIdRegex = new Regex(@"^[\dA-z]+$", RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public string Id { get; }

        public string AbbreviatedId { get; }


        public GitId(string id, string abbreviatedId)
        {
            if (String.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Value must not be null or whitespace", nameof(id));

            if (!s_ObjectIdRegex.IsMatch(id))
                throw new ArgumentException($"'{id}' is not a valid git object id", nameof(id));

            if (id.Length != 40)
                throw new ArgumentException($"'{id}' is not a full, 40 character git object id", nameof(id));

            if (String.IsNullOrWhiteSpace(abbreviatedId))
                throw new ArgumentException("Value must not be null or whitespace", nameof(abbreviatedId));

            if (!s_ObjectIdRegex.IsMatch(abbreviatedId))
                throw new ArgumentException($"'{abbreviatedId}' is not a valid git object id", nameof(abbreviatedId));

            if (!id.StartsWith(abbreviatedId, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"Mismtach between abbreviated id '{abbreviatedId}' and full id '{id}'");

            Id = id;
            AbbreviatedId = abbreviatedId;
        }


        public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Id);

        public override bool Equals(object? obj) => obj is GitId otherId && Equals(otherId);

        public bool Equals(GitId other) => StringComparer.OrdinalIgnoreCase.Equals(Id, other.Id);

        public override string ToString() => Id.ToLower();

        public string ToString(bool abbreviate) => abbreviate ? AbbreviatedId : Id;

        public static bool operator ==(GitId left, GitId right) => left.Equals(right);

        public static bool operator !=(GitId left, GitId right) => !left.Equals(right);
    }
}
