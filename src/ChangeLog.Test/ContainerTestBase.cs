using System;
using Autofac;

namespace Grynwald.ChangeLog.Test
{
    public class ContainerTestBase
    {
        protected IContainer BuildContainer(Action<ContainerBuilder> setup)
        {
            var containerBuilder = new ContainerBuilder();
            setup(containerBuilder);

            return containerBuilder.Build();
        }
    }
}
