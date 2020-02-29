using System.Collections.Generic;

namespace ChangeLogCreator.Versions
{
    internal interface IVersionProvider
    {
        IReadOnlyList<VersionInfo> AllVersions { get; }
    }
}
