using Autofac;
using Grynwald.ChangeLog.Integrations.GitHub;
using Grynwald.ChangeLog.Integrations.GitLab;
using Grynwald.ChangeLog.Pipeline;

namespace Grynwald.ChangeLog.Integrations
{
    internal static class IntegrationsExtensions
    {
        public static void RegisterIntegrations(this ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<GitHubClientFactory>().As<IGitHubClientFactory>();
            containerBuilder.RegisterType<GitHubLinkTask>().As<IChangeLogTask>();

            containerBuilder.RegisterType<GitLabClientFactory>().As<IGitLabClientFactory>();
            containerBuilder.RegisterType<GitLabLinkTask>().As<IChangeLogTask>();
        }
    }
}
