using Autofac;
using Grynwald.ChangeLog.Configuration;
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
            containerBuilder.RegisterType<GitHubLinkTask>();

            containerBuilder.RegisterType<GitLabClientFactory>().As<IGitLabClientFactory>();
            containerBuilder.RegisterType<GitLabLinkTask>();
        }


        public static IChangeLogPipelineBuilder AddIntegrationTasks(this IChangeLogPipelineBuilder pipelineBuilder)
        {
            var configuration = pipelineBuilder.Container.Resolve<ChangeLogConfiguration>();

            if (configuration.Integrations.Provider == ChangeLogConfiguration.IntegrationProvider.GitHub)
            {
                pipelineBuilder = pipelineBuilder.AddTask<GitHubLinkTask>();
            }
            else if (configuration.Integrations.Provider == ChangeLogConfiguration.IntegrationProvider.GitLab)
            {
                pipelineBuilder = pipelineBuilder.AddTask<GitLabLinkTask>();
            }

            return pipelineBuilder;
        }
    }
}
