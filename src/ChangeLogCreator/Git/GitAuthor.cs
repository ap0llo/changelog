using System;
using System.Diagnostics.CodeAnalysis;

namespace ChangeLogCreator.Git
{
    public sealed class GitAuthor : IEquatable<GitAuthor>
    {
        public string Name { get; }

        public string Email { get; }


        public GitAuthor(string name, string email)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Value must not be null or whitespace", nameof(name));

            if (String.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Value must not be null or whitespace", nameof(email));

            Name = name;
            Email = email;
        }


        public override bool Equals(object? obj) => Equals(obj as GitAuthor);

        public override int GetHashCode() => HashCode.Combine(Name, Email);

        public bool Equals([AllowNull] GitAuthor other) =>
            other != null &&
            StringComparer.Ordinal.Equals(Name, other.Name) &&
            StringComparer.Ordinal.Equals(Email, other.Email);
    }
}
