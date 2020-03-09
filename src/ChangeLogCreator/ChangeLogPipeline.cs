using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChangeLogCreator.Model;
using ChangeLogCreator.Tasks;

namespace ChangeLogCreator
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

        public async Task<ChangeLog> RunAsync()
        {
            var changeLog = new ChangeLog();
            foreach (var task in m_Tasks)
            {
                await task.RunAsync(changeLog);
            }
            return changeLog;
        }
    }
}
