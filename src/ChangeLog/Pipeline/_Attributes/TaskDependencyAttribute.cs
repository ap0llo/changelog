using System;

namespace Grynwald.ChangeLog.Pipeline
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    internal abstract class TaskDependencyAttribute : Attribute
    {
        public Type TaskType { get; }

        public TaskDependencyAttribute(Type taskType)
        {
            TaskType = taskType ?? throw new ArgumentNullException(nameof(taskType));
        }
    }
}
