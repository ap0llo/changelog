using Autofac;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.Templates.Default;

namespace Grynwald.ChangeLog.Templates
{
    internal static class TemplateContainerBuildExtensions
    {
        public static void RegisterTemplate(this ContainerBuilder containerBuilder, ChangeLogConfiguration.TemplateConfiguration configuration)
        {
            containerBuilder.RegisterType<DefaultTemplate>().As<ITemplate>();
        }
    }
}
