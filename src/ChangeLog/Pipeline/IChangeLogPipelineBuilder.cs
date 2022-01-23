using System.Collections.Generic;
using Autofac;

namespace Grynwald.ChangeLog.Pipeline
{
    internal interface IChangeLogPipelineBuilder
    {
        IContainer Container { get; }

        IEnumerable<IChangeLogTask> Tasks { get; }

        IChangeLogPipelineBuilder AddTask<T>() where T : IChangeLogTask;

        ChangeLogPipeline Build();
    }
}
