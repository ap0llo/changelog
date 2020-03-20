using Autofac;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.Integrations.GitHub;

namespace Grynwald.ChangeLog.Integrations
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
