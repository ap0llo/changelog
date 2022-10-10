using Autofac;
using Grynwald.ChangeLog.Templates.Default;
using Grynwald.ChangeLog.Templates.GitHubRelease;
using Grynwald.ChangeLog.Templates.GitLabRelease;
using Grynwald.ChangeLog.Templates.Html;

namespace Grynwald.ChangeLog.Templates
{
    internal static class TemplateContainerBuildExtensions
    {
        public static void RegisterTemplates(this ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<DefaultTemplate>().As<ITemplate>();
            containerBuilder.RegisterType<GitLabReleaseTemplate>().As<ITemplate>();
            containerBuilder.RegisterType<GitHubReleaseTemplate>().As<ITemplate>();
            containerBuilder.RegisterType<HtmlTemplate>().As<ITemplate>();
        }
    }
}
