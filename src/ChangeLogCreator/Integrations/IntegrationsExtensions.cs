using Autofac;
using ChangeLogCreator.Configuration;
using ChangeLogCreator.Integrations.GitHub;

namespace ChangeLogCreator.Integrations
{
    internal static class IntegrationsExtensions
    {
        public static void RegisterIntegrations(this ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<GitHubClientFactory>().As<IGitHubClientFactory>();
            containerBuilder.RegisterType<GitHubLinkTask>();
        }


        public static ChangeLogPipelineBuilder AddIntegrationTasks(this ChangeLogPipelineBuilder pipelineBuilder)
        {
            var configuration = pipelineBuilder.Container.Resolve<ChangeLogConfiguration>();

            if (configuration.Integrations.Provider == ChangeLogConfiguration.IntegrationProvider.GitHub)
            {
                pipelineBuilder = pipelineBuilder.AddTask<GitHubLinkTask>();
            }

            return pipelineBuilder;
        }
    }
}
