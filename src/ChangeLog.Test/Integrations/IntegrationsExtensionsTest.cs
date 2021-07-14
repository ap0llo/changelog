using System.Linq;
using Autofac;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.Git;
using Grynwald.ChangeLog.Integrations;
using Grynwald.ChangeLog.Integrations.GitHub;
using Grynwald.ChangeLog.Integrations.GitLab;
using Grynwald.ChangeLog.Pipeline;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Grynwald.ChangeLog.Test.Integrations
{
    /// <summary>
    /// Tests for <see cref="IntegrationsExtensions"/>
    /// </summary>
    public class IntegrationsExtensionsTest : ContainerTestBase
    {
        [Fact]
        public void RegisterIntegrations_registers_expected_types()
        {
            // ARRANGE / ACT
            var gitRepositoryMock = new Mock<IGitRepository>(MockBehavior.Strict);
            gitRepositoryMock.Setup(x => x.Remotes).Returns(Enumerable.Empty<GitRemote>());

            var container = BuildContainer(b =>
            {
                // register dependencies
                b.RegisterInstance(gitRepositoryMock.Object).As<IGitRepository>();
                b.RegisterInstance(new ChangeLogConfiguration());
                b.RegisterInstance(NullLogger<GitHubLinkTask>.Instance).As<ILogger<GitHubLinkTask>>();
                b.RegisterInstance(NullLogger<GitLabLinkTask>.Instance).As<ILogger<GitLabLinkTask>>();

                b.RegisterIntegrations();
            });

            // ASSERT
            AutofacAssert.CanResolveType<IGitHubClientFactory>(container);
            AutofacAssert.CanResolveType<GitHubLinkTask>(container);

            AutofacAssert.CanResolveType<IGitLabClientFactory>(container);
            AutofacAssert.CanResolveType<GitLabLinkTask>(container);
        }

        [Theory]
        [EnumData]
        public void AddIntegrationTasks_adds_expected_tasks(ChangeLogConfiguration.IntegrationProvider integrationProvider)
        {
            // ARRANGE            
            var configuration = new ChangeLogConfiguration();
            configuration.Integrations.Provider = integrationProvider;

            using var container = BuildContainer(b => b.RegisterInstance(configuration));

            var pipelineBuilderMock = new Mock<IChangeLogPipelineBuilder>(MockBehavior.Strict);
            pipelineBuilderMock.Setup(x => x.Container).Returns(container);
            pipelineBuilderMock.Setup(x => x.AddTask<GitHubLinkTask>()).Returns(pipelineBuilderMock.Object);
            pipelineBuilderMock.Setup(x => x.AddTask<GitLabLinkTask>()).Returns(pipelineBuilderMock.Object);

            // ACT
            pipelineBuilderMock.Object.AddIntegrationTasks();

            // ASSERT
            pipelineBuilderMock.Verify(
                x => x.AddTask<GitHubLinkTask>(),
                integrationProvider == ChangeLogConfiguration.IntegrationProvider.GitHub ? Times.Once() : Times.Never()
            );
            pipelineBuilderMock.Verify(
                x => x.AddTask<GitLabLinkTask>(),
                integrationProvider == ChangeLogConfiguration.IntegrationProvider.GitLab ? Times.Once() : Times.Never()
            );
        }


    }
}
