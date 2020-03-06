using System;
using System.Diagnostics.CodeAnalysis;
using ChangeLogCreator.Git;
using NuGet.Versioning;

namespace ChangeLogCreator.Model
{
    public sealed class VersionInfo : IEquatable<VersionInfo>
    {
        public SemanticVersion Version { get; }

        public GitId Commit { get; }


        public VersionInfo(SemanticVersion version, GitId commit)
        {
            Version = version ?? throw new ArgumentNullException(nameof(version));
            Commit = commit;
        }


        public override int GetHashCode() => HashCode.Combine(Version, Commit);

        public override bool Equals(object? obj) => Equals(obj as VersionInfo);

        public bool Equals([AllowNull] VersionInfo other) =>
            other != null &&
            Version.Equals(other.Version) &&
            Commit.Equals(other.Commit);
    }
}
