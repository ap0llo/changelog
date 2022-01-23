using System;
using System.Threading.Tasks;
using Grynwald.ChangeLog.Git;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Pipeline;
using Grynwald.ChangeLog.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace Grynwald.ChangeLog.Test.Tasks
{
    /// <summary>
    /// Unit tests for <see cref="LoadCommitsTask"/>
    /// </summary>
    public class LoadCommitsTaskTest : TestBase
    {
        private readonly ILogger<LoadCommitsTask> m_Logger;


        public LoadCommitsTaskTest(ITestOutputHelper testOutputHelper)
        {
            m_Logger = new XunitLogger<LoadCommitsTask>(testOutputHelper);
        }


        [Fact]
        public void Logger_must_not_be_null()
        {
            // ARRANGE

            // ACT 
            var ex = Record.Exception(() => new LoadCommitsTask(logger: null!, repository: Mock.Of<IGitRepository>()));

            // ASSERT
            var argumentNullException = Assert.IsType<ArgumentNullException>(ex);
            Assert.Equal("logger", argumentNullException.ParamName);
        }

        [Fact]
        public void Repository_must_not_be_null()
        {
            // ARRANGE

            // ACT 
            var ex = Record.Exception(() => new LoadCommitsTask(logger: m_Logger, repository: null!));

            // ASSERT
            var argumentNullException = Assert.IsType<ArgumentNullException>(ex);
            Assert.Equal("repository", argumentNullException.ParamName);
        }

        [Fact]
        public async Task Run_does_nothing_for_empty_changelog()
        {
            // ARRANGE
            var repo = Mock.Of<IGitRepository>(MockBehavior.Strict);

            var sut = new LoadCommitsTask(m_Logger, repo);

            // ACT
            var changelog = new ApplicationChangeLog();
            var result = await sut.RunAsync(changelog);

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Skipped, result);
        }

        [Fact]
        public async Task Run_adds_all_commits_if_no_previous_version_exists()
        {
            // ARRANGE
            var repo = new Mock<IGitRepository>(MockBehavior.Strict);
            repo
                .Setup(x => x.GetCommits(null, It.IsAny<GitId>()))
                .Returns(new[]
                {
                    GetGitCommit(TestGitIds.Id1, "Commit 1"),
                    GetGitCommit(TestGitIds.Id2, "Commit 2")
                });

            var sut = new LoadCommitsTask(m_Logger, repo.Object);

            var versionChangeLog = GetSingleVersionChangeLog("1.2.3", TestGitIds.Id1);
            var changelog = new ApplicationChangeLog() { versionChangeLog };

            // ACT
            var result = await sut.RunAsync(changelog);

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Success, result);

            repo.Verify(x => x.GetCommits(It.IsAny<GitId?>(), It.IsAny<GitId>()), Times.Once);

            Assert.NotNull(versionChangeLog.AllCommits);
            Assert.Equal(2, versionChangeLog.AllCommits.Count);
            Assert.Contains(versionChangeLog.AllCommits, c => c.Id == TestGitIds.Id1);
            Assert.Contains(versionChangeLog.AllCommits, c => c.Id == TestGitIds.Id2);
        }

        [Fact]
        public async Task Run_adds_the_expected_commits_if_a_previous_version_exists()
        {
            // ARRANGE
            var repo = new Mock<IGitRepository>(MockBehavior.Strict);
            repo
                .Setup(x => x.GetCommits(null, TestGitIds.Id1))
                .Returns(Array.Empty<GitCommit>());

            repo
                .Setup(x => x.GetCommits(TestGitIds.Id1, TestGitIds.Id2))
                .Returns(new[]
                {
                    GetGitCommit(TestGitIds.Id1, "Commit 1" ),
                    GetGitCommit(TestGitIds.Id2, "Commit 2" ),
                });

            var sut = new LoadCommitsTask(m_Logger, repo.Object);

            var versionChangeLog1 = GetSingleVersionChangeLog("1.2.3", TestGitIds.Id1);

            var versionChangeLog2 = GetSingleVersionChangeLog("2.4.5", TestGitIds.Id2);

            var changelog = new ApplicationChangeLog()
            {
                versionChangeLog1, versionChangeLog2
            };

            // ACT
            var result = await sut.RunAsync(changelog);

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Success, result);

            repo.Verify(x => x.GetCommits(null, It.IsAny<GitId>()), Times.Once);
            repo.Verify(x => x.GetCommits(It.IsAny<GitId>(), It.IsAny<GitId>()), Times.Once);

            Assert.NotNull(versionChangeLog1.AllCommits);
            Assert.Empty(versionChangeLog1.AllCommits);

            Assert.NotNull(versionChangeLog2.AllCommits);
            Assert.Equal(2, versionChangeLog2.AllCommits.Count);
            Assert.Contains(versionChangeLog2.AllCommits, c => c.Id == TestGitIds.Id1);
            Assert.Contains(versionChangeLog2.AllCommits, c => c.Id == TestGitIds.Id2);
        }
    }
}
