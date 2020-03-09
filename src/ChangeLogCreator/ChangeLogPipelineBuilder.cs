using System;
using System.Collections.Generic;
using Autofac;
using ChangeLogCreator.Tasks;

namespace ChangeLogCreator
{
    internal class ChangeLogPipelineBuilder
    {
        private readonly List<IChangeLogTask> m_Tasks;


        public IContainer Container { get; }


        public ChangeLogPipelineBuilder(IContainer container)
        {
            Container = container ?? throw new ArgumentNullException(nameof(container));
            m_Tasks = new List<IChangeLogTask>();
        }


        public ChangeLogPipelineBuilder AddTask<T>() where T : IChangeLogTask
        {
            var task = Container.Resolve<T>();
            m_Tasks.Add(task);
            return this;
        }


        public ChangeLogPipeline Build() => new ChangeLogPipeline(m_Tasks);
    }
}
