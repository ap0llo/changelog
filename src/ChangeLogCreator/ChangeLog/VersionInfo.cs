using System;
using System.Diagnostics.CodeAnalysis;
using ChangeLogCreator.Git;
using NuGet.Versioning;

namespace ChangeLogCreator.ChangeLog
{
    internal sealed class VersionInfo : IEquatable<VersionInfo>
    {
        public SemanticVersion Version { get; }

        public GitTag Tag { get; }


        public VersionInfo(SemanticVersion version, GitTag tag)
        {
            Version = version ?? throw new ArgumentNullException(nameof(version));
            Tag = tag ?? throw new ArgumentNullException(nameof(tag));
        }


        public override int GetHashCode() => HashCode.Combine(Version, Tag);

        public override bool Equals(object? obj) => Equals(obj as VersionInfo);

        public bool Equals([AllowNull] VersionInfo other) =>
            other != null && Version.Equals(other.Version) && Tag.Equals(other.Tag);
    }
}
