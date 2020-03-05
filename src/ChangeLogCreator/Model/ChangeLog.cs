using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ChangeLogCreator.Model
{
    public sealed class ChangeLog : IEnumerable<SingleVersionChangeLog>
    {
        private readonly Dictionary<VersionInfo, SingleVersionChangeLog> m_ChangeLogs = new Dictionary<VersionInfo, SingleVersionChangeLog>();


        public SingleVersionChangeLog this[VersionInfo version] => m_ChangeLogs[version];

        public IEnumerable<VersionInfo> Versions => m_ChangeLogs.Keys;

        public IEnumerable<SingleVersionChangeLog> ChangeLogs => m_ChangeLogs.Values.OrderByDescending(x => x.Version.Version);



        public void Add(SingleVersionChangeLog versionChangeLog)
        {
            m_ChangeLogs.Add(versionChangeLog.Version, versionChangeLog);
        }

        public IEnumerator<SingleVersionChangeLog> GetEnumerator() => ChangeLogs.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ChangeLogs.GetEnumerator();
    }
}
