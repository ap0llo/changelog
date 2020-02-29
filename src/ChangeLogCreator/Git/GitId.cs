using System;
using System.Text.RegularExpressions;

namespace ChangeLogCreator.Git
{
    public struct GitId : IEquatable<GitId>
    {
        private static readonly Regex s_ObjectIdRegex = new Regex(@"^[\dA-z]+$", RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public string Id { get; }


        public GitId(string id)
        {
            if (String.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Value must not be null or whitespace", nameof(id));

            if (!s_ObjectIdRegex.IsMatch(id))
                throw new ArgumentException($"'{id}' is not a valid git object id", nameof(id));

            Id = id;
        }


        public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Id);

        public override bool Equals(object? obj) => obj is GitId otherId && Equals(otherId);

        public bool Equals(GitId other) => StringComparer.OrdinalIgnoreCase.Equals(Id, other.Id);
    }
}
