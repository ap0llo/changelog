﻿using System.IO;
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
        private readonly ILogger<RenderTemplateTask> m_Logger;


        public RenderTemplateTaskTest(ITestOutputHelper testOutputHelper)
        {
            m_Logger = new XunitLogger<RenderTemplateTask>(testOutputHelper);
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

            var templateMock = new Mock<ITemplate>(MockBehavior.Strict);
            templateMock.Setup(x => x.SaveChangeLog(It.IsAny<ApplicationChangeLog>(), It.IsAny<string>()));

            var sut = new RenderTemplateTask(m_Logger, configuration, templateMock.Object);

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

            var templateMock = new Mock<ITemplate>(MockBehavior.Strict);
            templateMock
                .Setup(x => x.SaveChangeLog(It.IsAny<ApplicationChangeLog>(), It.IsAny<string>()))
                .Throws(new TemplateExecutionException("Irrelevant"));

            var sut = new RenderTemplateTask(m_Logger, configuration, templateMock.Object);

            // ACT
            var result = await sut.RunAsync(new ApplicationChangeLog());

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Error, result);
        }
    }
}
