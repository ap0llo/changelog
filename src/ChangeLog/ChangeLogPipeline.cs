using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Tasks;

namespace Grynwald.ChangeLog
{
    public sealed class ChangeLogPipeline
    {
        private readonly IReadOnlyList<IChangeLogTask> m_Tasks;


        public ChangeLogPipeline(IEnumerable<IChangeLogTask> tasks)
        {
            if (tasks is null)
                throw new ArgumentNullException(nameof(tasks));

            m_Tasks = tasks.ToList();
        }

        public async Task<ApplicationChangeLog> RunAsync()
        {
            var changeLog = new ApplicationChangeLog();
            foreach (var task in m_Tasks)
            {
                await task.RunAsync(changeLog);
            }
            return changeLog;
        }
    }
}
