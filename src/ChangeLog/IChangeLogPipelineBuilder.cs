using Autofac;
using Grynwald.ChangeLog.Tasks;

namespace Grynwald.ChangeLog
{
    internal interface IChangeLogPipelineBuilder
    {
        IContainer Container { get; }

        IChangeLogPipelineBuilder AddTask<T>() where T : IChangeLogTask;

        ChangeLogPipeline Build();
    }
}
