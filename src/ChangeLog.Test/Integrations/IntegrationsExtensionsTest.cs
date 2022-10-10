using System.Collections.Generic;
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
            var tasks = container.Resolve<IEnumerable<IChangeLogTask>>();
            Assert.Collection(
                tasks.OrderBy(x => x.GetType().Name),
                x => Assert.IsType<GitHubLinkTask>(x),
                x => Assert.IsType<GitLabLinkTask>(x)
            );
        }
    }
}
