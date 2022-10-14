using System;

namespace Grynwald.ChangeLog.Git
{
    public sealed class GitRemote : IEquatable<GitRemote>
    {
        public string Name { get; }

        public string Url { get; }


        public GitRemote(string name, string url)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Value must not be null or whitespace", nameof(name));

            if (String.IsNullOrWhiteSpace(url))
                throw new ArgumentException("Value must not be null or whitespace", nameof(url));

            Name = name;
            Url = url;
        }


        public override bool Equals(object? obj) => Equals(obj as GitRemote);

        public override int GetHashCode() => HashCode.Combine(Name, Url);

        public bool Equals(GitRemote? other) =>
            other is not null &&
            StringComparer.Ordinal.Equals(Name, other.Name) &&
            StringComparer.Ordinal.Equals(Url, other.Url);
    }
}
