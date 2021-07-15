using System;

namespace Grynwald.ChangeLog.Pipeline
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    internal sealed class AfterTaskAttribute : TaskDependencyAttribute
    {
        public AfterTaskAttribute(Type taskType) : base(taskType)
        { }
    }
}
