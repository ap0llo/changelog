using System.Threading.Tasks;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.Git;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Pipeline;
using Grynwald.ChangeLog.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NuGet.Versioning;
using Xunit;

namespace Grynwald.ChangeLog.Test.Tasks
{
    /// <summary>
    /// Unit tests for <see cref="LoadCurrentVersionTask"/>
    /// </summary>
    public class LoadCurrentVersionTaskTest : TestBase
    {
        private readonly ILogger<LoadCurrentVersionTask> m_Logger;
        private readonly Mock<IGitRepository> m_RepositoryMock;

        public LoadCurrentVersionTaskTest()
        {
            m_Logger = NullLogger<LoadCurrentVersionTask>.Instance;
            m_RepositoryMock = new Mock<IGitRepository>(MockBehavior.Strict);
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
    }
}
