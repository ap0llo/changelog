using System;

namespace Grynwald.ChangeLog.Pipeline
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    internal sealed class BeforeTaskAttribute : Attribute
    {
        public Type TaskType { get; }

        public BeforeTaskAttribute(Type taskType)
        {
            TaskType = taskType ?? throw new ArgumentNullException(nameof(taskType));
        }
    }
}
