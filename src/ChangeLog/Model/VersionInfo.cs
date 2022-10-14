using System;
using Grynwald.ChangeLog.Git;
using NuGet.Versioning;

namespace Grynwald.ChangeLog.Model
{
    public sealed class VersionInfo : IEquatable<VersionInfo>
    {
        public NuGetVersion Version { get; }

        public GitId Commit { get; }


        public VersionInfo(NuGetVersion version, GitId commit)
        {
            if (commit.IsNull)
                throw new ArgumentException("Commit must not be empty", nameof(commit));

            Version = version ?? throw new ArgumentNullException(nameof(version));
            Commit = commit;
        }


        public override int GetHashCode() => HashCode.Combine(Version, Commit);

        public override bool Equals(object? obj) => Equals(obj as VersionInfo);

        public bool Equals(VersionInfo? other) =>
            other is not null &&
            Version.Equals(other.Version) &&
            Commit.Equals(other.Commit);
    }
}
