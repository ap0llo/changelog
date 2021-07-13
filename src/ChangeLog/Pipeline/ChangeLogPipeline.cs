using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grynwald.ChangeLog.Model;
using Microsoft.Extensions.Logging;

namespace Grynwald.ChangeLog.Pipeline
{
    public sealed class ChangeLogPipeline
    {
        private readonly IReadOnlyList<IChangeLogTask> m_Tasks;
        private readonly ILogger<ChangeLogPipeline> m_Logger;


        public IEnumerable<IChangeLogTask> Tasks => m_Tasks;


        public ChangeLogPipeline(ILogger<ChangeLogPipeline> logger, IEnumerable<IChangeLogTask> tasks)
        {
            m_Tasks = (tasks ?? throw new ArgumentNullException(nameof(tasks))).ToList();
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        public async Task<ChangeLogPipelineResult> RunAsync()
        {
            var changeLog = new ApplicationChangeLog();
            var executedTasks = new List<ChangeLogTaskExecutionResult>();
            var pendingTasks = new Queue<IChangeLogTask>(m_Tasks);

            while (pendingTasks.Count > 0)
            {
                var task = pendingTasks.Dequeue();
                var taskName = task.GetType().Name;

                m_Logger.LogDebug($"Running task '{taskName}'");
                var result = await task.RunAsync(changeLog);
                m_Logger.LogDebug($"Task '{taskName}' completed with result '{result}'");

                executedTasks.Add(new ChangeLogTaskExecutionResult(task, result));

                if (result == ChangeLogTaskResult.Error)
                {
                    m_Logger.LogDebug($"Task '{taskName}' failed, aborting execution of pipeline.");
                    return ChangeLogPipelineResult.CreateErrorResult(executedTasks, pendingTasks.ToList());
                }
            }
            return ChangeLogPipelineResult.CreateSuccessResult(executedTasks, Array.Empty<IChangeLogTask>(), changeLog);
        }
    }
}
