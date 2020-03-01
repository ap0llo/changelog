using System;
using System.Collections.Generic;
using ChangeLogCreator.Model;

namespace ChangeLogCreator.Tasks
{
    public sealed class ChangeLogPipeline
    {
        private readonly List<IChangeLogTask> m_Tasks = new List<IChangeLogTask>();

        public ChangeLogPipeline()
        { }

        public ChangeLogPipeline AddTask(IChangeLogTask task)
        {
            if (task is null)
                throw new ArgumentNullException(nameof(task));

            m_Tasks.Add(task);

            return this;
        }


        public ChangeLog Run()
        {
            var changeLog = new ChangeLog();
            foreach (var task in m_Tasks)
            {
                task.Run(changeLog);
            }
            return changeLog;
        }
    }
}
