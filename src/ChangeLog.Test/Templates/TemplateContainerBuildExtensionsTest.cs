using System.Collections.Generic;
using System.Linq;
using Autofac;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.Templates;
using Grynwald.ChangeLog.Templates.Default;
using Grynwald.ChangeLog.Templates.GitHubRelease;
using Grynwald.ChangeLog.Templates.GitLabRelease;
using Grynwald.ChangeLog.Templates.Html;
using Xunit;

namespace Grynwald.ChangeLog.Test.Templates
{
    public class TemplateContainerBuildExtensionsTest
    {
        [Fact]
        public void RegisterTemplates_registers_the_expected_template_instances()
        {
            // ARRANGE
            var configuration = new ChangeLogConfiguration();

            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterInstance(configuration).SingleInstance();

            // ACT
            containerBuilder.RegisterTemplates();

            // ASSERT
            var container = containerBuilder.Build();
            var templates = container.Resolve<IEnumerable<ITemplate>>();

            Assert.Collection(
                templates.OrderBy(x => x.Name),
                x => Assert.IsType<DefaultTemplate>(x),
                x => Assert.IsType<GitLabReleaseTemplate>(x),
                x => Assert.IsType<GitHubReleaseTemplate>(x),
                x => Assert.IsType<HtmlTemplate>(x)
            );
        }
    }
}
