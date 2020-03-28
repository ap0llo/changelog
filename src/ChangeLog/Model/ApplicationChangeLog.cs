using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NuGet.Versioning;

namespace Grynwald.ChangeLog.Model
{
    public sealed class ApplicationChangeLog : IEnumerable<SingleVersionChangeLog>
    {
        private readonly HashSet<NuGetVersion> m_Versions = new HashSet<NuGetVersion>();
        private readonly Dictionary<VersionInfo, SingleVersionChangeLog> m_ChangeLogs = new Dictionary<VersionInfo, SingleVersionChangeLog>();


        public SingleVersionChangeLog this[VersionInfo version] => m_ChangeLogs[version];

        public IEnumerable<VersionInfo> Versions => m_ChangeLogs.Keys;

        public IEnumerable<SingleVersionChangeLog> ChangeLogs => m_ChangeLogs.Values.OrderByDescending(x => x.Version.Version);


        public void Add(SingleVersionChangeLog versionChangeLog)
        {
            if (versionChangeLog is null)
                throw new ArgumentNullException(nameof(versionChangeLog));

            if (m_Versions.Contains(versionChangeLog.Version.Version))
                throw new InvalidOperationException($"Changelog already contains version '{versionChangeLog.Version.Version}'");

            m_ChangeLogs.Add(versionChangeLog.Version, versionChangeLog);
            m_Versions.Add(versionChangeLog.Version.Version);
        }

        public void Remove(SingleVersionChangeLog versionChangeLog) => m_ChangeLogs.Remove(versionChangeLog.Version);

        public bool ContainsVersion(NuGetVersion version) => m_Versions.Contains(version);

        public IEnumerator<SingleVersionChangeLog> GetEnumerator() => ChangeLogs.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ChangeLogs.GetEnumerator();
    }
}
