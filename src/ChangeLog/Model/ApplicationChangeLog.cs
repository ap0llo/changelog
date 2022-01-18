using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NuGet.Versioning;

namespace Grynwald.ChangeLog.Model
{
    public sealed class ApplicationChangeLog : IEnumerable<SingleVersionChangeLog>
    {
        private readonly Dictionary<NuGetVersion, SingleVersionChangeLog> m_ChangeLogsByVersion = new();
        private readonly Dictionary<VersionInfo, SingleVersionChangeLog> m_ChangeLogsByVersionInfo = new();


        public SingleVersionChangeLog this[VersionInfo versionInfo] => m_ChangeLogsByVersionInfo[versionInfo];

        public SingleVersionChangeLog this[NuGetVersion version] => m_ChangeLogsByVersion[version];

        public IEnumerable<VersionInfo> Versions => m_ChangeLogsByVersionInfo.Keys;

        public IEnumerable<SingleVersionChangeLog> ChangeLogs => m_ChangeLogsByVersionInfo.Values.OrderByDescending(x => x.Version.Version);


        public void Add(SingleVersionChangeLog versionChangeLog)
        {
            if (versionChangeLog is null)
                throw new ArgumentNullException(nameof(versionChangeLog));

            if (m_ChangeLogsByVersion.ContainsKey(versionChangeLog.Version.Version))
                throw new InvalidOperationException($"Changelog already contains version '{versionChangeLog.Version.Version}'");

            m_ChangeLogsByVersionInfo.Add(versionChangeLog.Version, versionChangeLog);
            m_ChangeLogsByVersion.Add(versionChangeLog.Version.Version, versionChangeLog);
        }

        public void Remove(SingleVersionChangeLog versionChangeLog) => m_ChangeLogsByVersionInfo.Remove(versionChangeLog.Version);

        public bool ContainsVersion(NuGetVersion version) => m_ChangeLogsByVersion.ContainsKey(version);

        public IEnumerator<SingleVersionChangeLog> GetEnumerator() => ChangeLogs.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ChangeLogs.GetEnumerator();
    }
}
