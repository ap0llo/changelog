using System;

namespace Grynwald.ChangeLog.Pipeline
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    internal sealed class AfterTaskAttribute : Attribute
    {
        public Type TaskType { get; }

        public AfterTaskAttribute(Type taskType)
        {
            TaskType = taskType ?? throw new ArgumentNullException(nameof(taskType));
        }
    }
}
