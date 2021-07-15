using System;

namespace Grynwald.ChangeLog.Pipeline
{
    internal sealed class BeforeTaskAttribute : TaskDependencyAttribute
    {
        public BeforeTaskAttribute(Type taskType) : base(taskType)
        { }
    }
}
