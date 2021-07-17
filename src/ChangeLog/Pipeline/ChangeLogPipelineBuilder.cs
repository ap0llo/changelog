using System;
using System.Collections.Generic;
using Autofac;
using Grynwald.ChangeLog.Tasks;

namespace Grynwald.ChangeLog.Pipeline
{
    internal class ChangeLogPipelineBuilder : IChangeLogPipelineBuilder
    {
        private readonly List<IChangeLogTask> m_Tasks;


        public IContainer Container { get; }

        public IEnumerable<IChangeLogTask> Tasks => m_Tasks;


        public ChangeLogPipelineBuilder(IContainer container)
        {
            Container = container ?? throw new ArgumentNullException(nameof(container));
            m_Tasks = new List<IChangeLogTask>();
        }


        public IChangeLogPipelineBuilder AddTask<T>() where T : IChangeLogTask
        {
            var task = Container.Resolve<T>();
            m_Tasks.Add(task);
            return this;
        }


        public ChangeLogPipeline Build()
        {
            return Container.Resolve<ChangeLogPipeline>(
                TypedParameter.From<IEnumerable<IChangeLogTask>>(m_Tasks)
            );
        }
    }
}
