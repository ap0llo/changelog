using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Tasks;
using Microsoft.Extensions.Logging;

namespace Grynwald.ChangeLog
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
            foreach (var task in m_Tasks)
            {
                var taskName = task.GetType().Name;

                m_Logger.LogDebug($"Running task '{taskName}'");
                var result = await task.RunAsync(changeLog);
                m_Logger.LogDebug($"Task '{taskName}' completed with result '{result}'");

                if (result == ChangeLogTaskResult.Error)
                {
                    m_Logger.LogDebug($"Task '{taskName}' failed, aborting execution of pipeline.");
                    return ChangeLogPipelineResult.CreateErrorResult();
                }
            }
            return ChangeLogPipelineResult.CreateSuccessResult(changeLog);
        }
    }
}
