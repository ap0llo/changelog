using System;
using System.Linq;
using Autofac;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.Templates;
using Grynwald.ChangeLog.Templates.Default;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Grynwald.ChangeLog.Test.Templates
{
    public class TemplateContainerBuildExtensionsTest
    {
        [Theory]
        [CombinatorialData]
        public void RegisterTemplate_can_register_a_template_for_every_supported_template_name(ChangeLogConfiguration.TemplateName templateName)
        {
            // ARRANGE
            var configuration = new ChangeLogConfiguration()
            {
                Template = new ChangeLogConfiguration.TemplateConfiguration() { Name = templateName }
            };

            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterInstance(configuration).SingleInstance();
            containerBuilder.RegisterInstance(NullLogger<DefaultTemplate>.Instance).As<ILogger<DefaultTemplate>>();

            // ACT
            containerBuilder.RegisterTemplate(configuration.Template);

            // ASSERT
            var container = containerBuilder.Build();
            var template = container.Resolve<ITemplate>();
            Assert.NotNull(template);
        }

        [Fact]
        public void RegisterTemplate_throws_InvalidTemplateConfigurationException_for_invalid_template_Swetting()
        {
            // ARRANGE
            var templateName = (ChangeLogConfiguration.TemplateName)
                Enum.GetValues(typeof(ChangeLogConfiguration.TemplateName))
                    .Cast<int>()
                    .Max() + 2;

            var configuration = new ChangeLogConfiguration()
            {
                Template = new ChangeLogConfiguration.TemplateConfiguration() { Name = templateName }
            };

            var containerBuilder = new ContainerBuilder();

            // ACT / ASSERT
            Assert.Throws<InvalidTemplateConfigurationException>(() => containerBuilder.RegisterTemplate(configuration.Template));
        }
    }
}
