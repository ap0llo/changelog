using System;
using System.Threading.Tasks;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.Git;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Pipeline;
using Grynwald.ChangeLog.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NuGet.Versioning;
using Xunit;
using Xunit.Abstractions;

namespace Grynwald.ChangeLog.Test.Tasks
{
    /// <summary>
    /// Unit tests for <see cref="LoadCurrentVersionTask"/>
    /// </summary>
    public class LoadCurrentVersionTaskTest : TestBase
    {
        private readonly ILogger<LoadCurrentVersionTask> m_Logger;
        private readonly Mock<IGitRepository> m_RepositoryMock;


        public LoadCurrentVersionTaskTest(ITestOutputHelper testOutputHelper)
        {
            m_Logger = new XunitLogger<LoadCurrentVersionTask>(testOutputHelper);
            m_RepositoryMock = new Mock<IGitRepository>(MockBehavior.Strict);
        }


        [Fact]
        public void Logger_must_not_be_null()
        {
            // ARRANGE
            var config = new ChangeLogConfiguration();

            // ACT 
            var ex = Record.Exception(() => new LoadCurrentVersionTask(logger: null!, configuration: config, repository: m_RepositoryMock.Object));

            // ASSERT
            var argumentNullException = Assert.IsType<ArgumentNullException>(ex);
            Assert.Equal("logger", argumentNullException.ParamName);
        }

        [Fact]
        public void Configuration_must_not_be_null()
        {
            // ARRANGE

            // ACT 
            var ex = Record.Exception(() => new LoadCurrentVersionTask(logger: m_Logger, configuration: null!, repository: m_RepositoryMock.Object));

            // ASSERT
            var argumentNullException = Assert.IsType<ArgumentNullException>(ex);
            Assert.Equal("configuration", argumentNullException.ParamName);
        }

        [Fact]
        public void Repository_must_not_be_null()
        {
            // ARRANGE
            var config = new ChangeLogConfiguration();
            // ACT 
            var ex = Record.Exception(() => new LoadCurrentVersionTask(logger: m_Logger, configuration: config, repository: null!));

            // ASSERT
            var argumentNullException = Assert.IsType<ArgumentNullException>(ex);
            Assert.Equal("repository", argumentNullException.ParamName);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task Task_is_skipped_if_current_version_is_not_set(string currentVersion)
        {
            // ARRANGE
            var config = new ChangeLogConfiguration()
            {
                CurrentVersion = currentVersion
            };

            var sut = new LoadCurrentVersionTask(m_Logger, config, m_RepositoryMock.Object);

            var changeLog = new ApplicationChangeLog();

            // ACT 
            var result = await sut.RunAsync(changeLog);

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Skipped, result);
            Assert.Empty(changeLog.Versions);
        }

        [Theory]
        [InlineData(" ")]
        [InlineData("\t")]
        [InlineData("not-a-version")]
        [InlineData("v1.2.3")]
        public async Task Task_fails_if_current_version_is_set_but_cannot_be_parsed(string currentVersion)
        {
            // ARRANGE
            var config = new ChangeLogConfiguration()
            {
                CurrentVersion = currentVersion
            };

            var sut = new LoadCurrentVersionTask(m_Logger, config, m_RepositoryMock.Object);

            var changeLog = new ApplicationChangeLog();

            // ACT 
            var result = await sut.RunAsync(changeLog);

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Error, result);
            Assert.Empty(changeLog.Versions);
        }

        [Fact]
        public async Task Run_adds_expected_version_to_changelog()
        {
            // ARRANGE
            var config = new ChangeLogConfiguration()
            {
                CurrentVersion = "1.2.3"
            };

            var head = GetGitCommit(id: TestGitIds.Id1);

            m_RepositoryMock.Setup(x => x.Head).Returns(head);

            var sut = new LoadCurrentVersionTask(m_Logger, config, m_RepositoryMock.Object);

            var changeLog = new ApplicationChangeLog();

            // ACT 
            var result = await sut.RunAsync(changeLog);

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Success, result);
            var addedVersion = Assert.Single(changeLog.Versions);
            Assert.Equal(NuGetVersion.Parse("1.2.3"), addedVersion.Version);
            Assert.Equal(head.Id, addedVersion.Commit);
        }

        [Fact]
        public async Task Run_fails_if_the_current_version_already_exists_in_the_change_log()
        {
            // ARRANGE
            var config = new ChangeLogConfiguration()
            {
                CurrentVersion = "1.2.3"
            };

            var head = GetGitCommit(id: TestGitIds.Id1);
            m_RepositoryMock.Setup(x => x.Head).Returns(head);

            var changeLog = new ApplicationChangeLog()
            {
                GetSingleVersionChangeLog(version: "1.2.3")
            };

            var sut = new LoadCurrentVersionTask(m_Logger, config, m_RepositoryMock.Object);

            // ACT 
            var result = await sut.RunAsync(changeLog);

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Error, result);
        }

    }
}
