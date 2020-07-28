using System;
using System.Diagnostics.CodeAnalysis;

namespace Grynwald.ChangeLog.Integrations.GitHub
{
    public sealed class GitHubProjectInfo : IEquatable<GitHubProjectInfo>
    {
        public string Host { get; }

        public string Owner { get; }

        public string Repository { get; }


        public GitHubProjectInfo(string host, string owner, string repository)
        {
            if (String.IsNullOrWhiteSpace(host))
                throw new ArgumentException("Value must not be null or whitespace", nameof(host));

            if (String.IsNullOrWhiteSpace(owner))
                throw new ArgumentException("Value must not be null or whitespace", nameof(owner));

            if (String.IsNullOrWhiteSpace(repository))
                throw new ArgumentException("Value must not be null or whitespace", nameof(repository));

            Host = host;
            Owner = owner;
            Repository = repository;
        }


        public override int GetHashCode()
        {
            unchecked
            {
                var hash = StringComparer.OrdinalIgnoreCase.GetHashCode(Host) * 397;
                hash ^= StringComparer.OrdinalIgnoreCase.GetHashCode(Owner);
                hash ^= StringComparer.OrdinalIgnoreCase.GetHashCode(Repository);
                return hash;
            }
        }

        public override bool Equals(object? obj) => Equals(obj as GitHubProjectInfo);

        public bool Equals([AllowNull] GitHubProjectInfo other)
        {
            return other != null &&
                StringComparer.OrdinalIgnoreCase.Equals(Host, other.Host) &&
                StringComparer.OrdinalIgnoreCase.Equals(Owner, other.Owner) &&
                StringComparer.OrdinalIgnoreCase.Equals(Repository, other.Repository);
        }
    }
}
