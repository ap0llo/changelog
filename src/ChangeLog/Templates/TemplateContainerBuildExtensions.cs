using Autofac;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.Templates.Default;

namespace Grynwald.ChangeLog.Templates
{
    internal static class TemplateContainerBuildExtensions
    {
        public static void RegisterTemplate(this ContainerBuilder containerBuilder, ChangeLogConfiguration.TemplateConfiguration configuration)
        {
            switch (configuration.Name)
            {
                case ChangeLogConfiguration.TemplateName.Default:
                    containerBuilder.RegisterType<DefaultTemplate>().As<ITemplate>();
                    break;

                default:
                    throw new InvalidTemplateConfigurationException($"Unknown template '{configuration.Name}'");
            }
        }
    }
}
