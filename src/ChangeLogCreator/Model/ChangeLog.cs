using System.Collections;
using System.Collections.Generic;

namespace ChangeLogCreator.Model
{
    public sealed class ChangeLog : IEnumerable<SingleVersionChangeLog>
    {
        private Dictionary<VersionInfo, SingleVersionChangeLog> m_ChangeLogs = new Dictionary<VersionInfo, SingleVersionChangeLog>();


        public SingleVersionChangeLog this[VersionInfo version] => m_ChangeLogs[version];

        public IEnumerable<VersionInfo> Versions => m_ChangeLogs.Keys;



        public void Add(SingleVersionChangeLog versionChangeLog)
        {
            m_ChangeLogs.Add(versionChangeLog.Version, versionChangeLog);
        }

        public IEnumerator<SingleVersionChangeLog> GetEnumerator() => m_ChangeLogs.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => m_ChangeLogs.Values.GetEnumerator();
    }
}
