using Autofac;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.Templates.Default;
using Grynwald.ChangeLog.Templates.GitHubRelease;
using Grynwald.ChangeLog.Templates.GitLabRelease;
using Grynwald.ChangeLog.Templates.Html;
using Grynwald.ChangeLog.Templates.Markdown;

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

                case ChangeLogConfiguration.TemplateName.GitLabRelease:
                    containerBuilder.RegisterType<GitLabReleaseTemplate>().As<ITemplate>();
                    break;

                case ChangeLogConfiguration.TemplateName.GitHubRelease:
                    containerBuilder.RegisterType<GitHubReleaseTemplate>().As<ITemplate>();
                    break;

                case ChangeLogConfiguration.TemplateName.Html:
                    containerBuilder.RegisterType<HtmlTemplate>().As<ITemplate>();
                    break;

                case ChangeLogConfiguration.TemplateName.Markdown:
                    containerBuilder.RegisterType<MarkdownTemplate>().As<ITemplate>();
                    break;

                default:
                    throw new InvalidTemplateConfigurationException($"Unknown template '{configuration.Name}'");
            }
        }
    }
}
