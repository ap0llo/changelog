using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Pipeline;
using Grynwald.ChangeLog.Tasks;
using Grynwald.ChangeLog.Templates;
using Grynwald.Utilities.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace Grynwald.ChangeLog.Test.Tasks
{
    /// <summary>
    /// Tests for <see cref="RenderTemplateTask"/>
    /// </summary>
    public class RenderTemplateTaskTest
    {
        private readonly XunitLogger<RenderTemplateTask> m_Logger;


        public RenderTemplateTaskTest(ITestOutputHelper testOutputHelper)
        {
            m_Logger = new XunitLogger<RenderTemplateTask>(testOutputHelper);
        }


        private Mock<ITemplate> CreateTemplateMock(TemplateName? templateName = null)
        {
            var templateMock = new Mock<ITemplate>(MockBehavior.Strict);

            templateMock
                .Setup(x => x.Name)
                .Returns(templateName ?? TemplateName.Default);

            templateMock
                .Setup(x => x.SaveChangeLog(It.IsAny<ApplicationChangeLog>(), It.IsAny<string>()));

            return templateMock;
        }


        [Theory]
        [CombinatorialData]
        public async Task Run_uses_the_configured_template(TemplateName configuredTemplateName)
        {
            // ARRANGE
            using var temporaryDirectory = new TemporaryDirectory();
            var outputDirectory = Path.Combine(temporaryDirectory, "dir1");

            var configuration = new ChangeLogConfiguration()
            {
                OutputPath = Path.Combine(outputDirectory, "changelog.md"),
                Template = new()
                {
                    Name = configuredTemplateName
                }
            };


            var templates = new List<ITemplate>();
            var activeTemplateMock = default(Mock<ITemplate>);
#if NETCOREAPP3_1
            foreach (var name in Enum.GetValues(typeof(TemplateName)).Cast<TemplateName>())
#else
            foreach(var name in Enum.GetValues<TemplateName>())
#endif
            {
                var mock = CreateTemplateMock(name);
                templates.Add(mock.Object);

                if (name == configuredTemplateName)
                    activeTemplateMock = mock;
            }


            var sut = new RenderTemplateTask(m_Logger, configuration, templates);

            // ACT
            var result = await sut.RunAsync(new ApplicationChangeLog());

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Success, result);
            activeTemplateMock!.Verify(x => x.SaveChangeLog(It.IsAny<ApplicationChangeLog>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Run_returns_error_if_configured_template_cannot_be_found()
        {
            // ARRANGE
            using var temporaryDirectory = new TemporaryDirectory();
            var configuration = new ChangeLogConfiguration()
            {
                OutputPath = Path.Combine(temporaryDirectory, "changelog.md"),
                Template = new()
                {
                    Name = TemplateName.GitHubRelease
                }
            };

            var templates = new[]
            {
                CreateTemplateMock(TemplateName.Default).Object,
                CreateTemplateMock(TemplateName.Html).Object,
            };

            var sut = new RenderTemplateTask(m_Logger, configuration, templates);

            // ACT
            var result = await sut.RunAsync(new ApplicationChangeLog());

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Error, result);
            Assert.Contains(m_Logger.LoggedMessages, msg => msg.Contains("Template 'GitHubRelease' was not found"));
        }

        [Fact]
        public async Task Run_returns_error_if_multiple_matching_templates_are_found()
        {
            // ARRANGE
            using var temporaryDirectory = new TemporaryDirectory();
            var configuration = new ChangeLogConfiguration()
            {
                OutputPath = Path.Combine(temporaryDirectory, "changelog.md"),
                Template = new()
                {
                    Name = TemplateName.GitHubRelease
                }
            };

            var templates = new[]
            {
                CreateTemplateMock(TemplateName.GitHubRelease).Object,
                CreateTemplateMock(TemplateName.GitHubRelease).Object,
            };

            var sut = new RenderTemplateTask(m_Logger, configuration, templates);

            // ACT
            var result = await sut.RunAsync(new ApplicationChangeLog());

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Error, result);
            Assert.Contains(m_Logger.LoggedMessages, msg => msg.Contains("Found multiple templates named 'GitHubRelease'"));
        }

        [Fact]
        public async Task Run_creates_the_output_directory_before_calling_the_template()
        {
            // ARRANGE
            using var temporaryDirectory = new TemporaryDirectory();
            var outputDirectory = Path.Combine(temporaryDirectory, "dir1");

            var configuration = new ChangeLogConfiguration()
            {
                OutputPath = Path.Combine(outputDirectory, "changelog.md")
            };

            var templateMock = CreateTemplateMock();

            var sut = new RenderTemplateTask(m_Logger, configuration, new[] { templateMock.Object });

            // ACT
            var result = await sut.RunAsync(new ApplicationChangeLog());

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Success, result);
            Assert.True(Directory.Exists(outputDirectory));
            templateMock.Verify(x => x.SaveChangeLog(It.IsAny<ApplicationChangeLog>(), It.IsAny<string>()), Times.Once);
            templateMock.Verify(x => x.SaveChangeLog(It.IsAny<ApplicationChangeLog>(), configuration.OutputPath), Times.Once);
        }

        [Fact]
        public async Task Run_returns_error_if_template_throws_TemplateExecutionException()
        {
            // ARRANGE
            using var temporaryDirectory = new TemporaryDirectory();
            var configuration = new ChangeLogConfiguration()
            {
                OutputPath = Path.Combine(temporaryDirectory, "changelog.md")
            };

            var templateMock = CreateTemplateMock();
            templateMock
                .Setup(x => x.SaveChangeLog(It.IsAny<ApplicationChangeLog>(), It.IsAny<string>()))
                .Throws(new TemplateExecutionException("Irrelevant"));

            var sut = new RenderTemplateTask(m_Logger, configuration, new[] { templateMock.Object });

            // ACT
            var result = await sut.RunAsync(new ApplicationChangeLog());

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Error, result);
        }
    }
}
