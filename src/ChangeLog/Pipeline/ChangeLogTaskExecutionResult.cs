using System;

namespace Grynwald.ChangeLog.Pipeline
{
    /// <summary>
    /// Encapsulates information about the execution of a <see cref="IChangeLogTask" />
    /// </summary>
    public class ChangeLogTaskExecutionResult
    {
        /// <summary>
        /// The task this result belongs to.
        /// </summary>
        public IChangeLogTask Task { get; }

        /// <summary>
        /// The result of the task's execution.
        /// </summary>
        public ChangeLogTaskResult Result { get; }


        /// <summary>
        /// Initializes a new instance of <see cref="ChangeLogTaskExecutionResult"/>
        /// </summary>
        /// <param name="task">The task this result belongs to.</param>
        /// <param name="result">The result of the task's execution.</param>
        public ChangeLogTaskExecutionResult(IChangeLogTask task, ChangeLogTaskResult result)
        {
            Task = task ?? throw new ArgumentNullException(nameof(task));
            Result = result;
        }
    }
}
