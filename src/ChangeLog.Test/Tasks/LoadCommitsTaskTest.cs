using System;
using System.Linq;
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
                .Setup(x => x.GetCommits(null, TestGitIds.Id2))
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

            Assert.NotNull(versionChangeLog1.AllCommits);
            Assert.Empty(versionChangeLog1.AllCommits);

            Assert.NotNull(versionChangeLog2.AllCommits);
            Assert.Equal(2, versionChangeLog2.AllCommits.Count);
            Assert.Contains(versionChangeLog2.AllCommits, c => c.Id == TestGitIds.Id1);
            Assert.Contains(versionChangeLog2.AllCommits, c => c.Id == TestGitIds.Id2);
        }

        [Fact]
        public async Task Run_adds_only_the_commits_to_a_version_that_are_not_included_by_earlier_versions()
        {
            // ARRANGE
            var repo = new Mock<IGitRepository>(MockBehavior.Strict);

            // Scenario: There is an overlap of commits between the current version (N) and a previous version (N - 2),
            // but not between the immediate previous version (N-1) and version N-2.
            //
            // (Commit 4)  * [tag: v1.2] (version N)
            //             |                               (Commit 7) * [tag v1.1] (version N -1)
            // (Commit 3)  *                                          |
            //             |                               (Commit 6) *
            //             |                                          |
            // (Commit 2)  * [tag: v1.0] (version N - 2)              |
            //             |                               (Commit 5) *
            //             |                                          |
            // (Commit 1)  *------------------------------------------+
            //             |
            //             |

            var commit1 = GetGitCommit(TestGitIds.Id1, "Commit 1");
            var commit2 = GetGitCommit(TestGitIds.Id2, "Commit 2");
            var commit3 = GetGitCommit(TestGitIds.Id3, "Commit 3");
            var commit4 = GetGitCommit(TestGitIds.Id4, "Commit 4");
            var commit5 = GetGitCommit(TestGitIds.Id5, "Commit 5");
            var commit6 = GetGitCommit(TestGitIds.Id6, "Commit 6");
            var commit7 = GetGitCommit(TestGitIds.Id7, "Commit 7");


            // Setup commits for version N
            repo
                .Setup(x => x.GetCommits(null, TestGitIds.Id4))
                .Returns(
                    [
                        commit4,
                        commit3,
                        commit2,
                        commit1,
                    ]);

            repo
                .Setup(x => x.GetCommits(TestGitIds.Id2, TestGitIds.Id4))
                .Returns(
                [
                    commit4,
                    commit3,
                ]);

            repo
                .Setup(x => x.GetCommits(TestGitIds.Id7, TestGitIds.Id4))
                .Returns(
                [
                    commit4,
                    commit3,
                    commit2,
                ]);


            // Setup commits for version N - 1
            repo
                .Setup(x => x.GetCommits(null, TestGitIds.Id7))
                .Returns(
                [
                    commit7,
                    commit6,
                    commit5,
                    commit1,
                ]);

            repo
                .Setup(x => x.GetCommits(TestGitIds.Id2, TestGitIds.Id7))
                .Returns(
                [
                    commit7,
                    commit6,
                    commit5,
                ]);



            // Setup commits for version N - 2
            repo
                .Setup(x => x.GetCommits(null, TestGitIds.Id2))
                .Returns(
                [
                    commit2,
                    commit1,
                ]);

            var sut = new LoadCommitsTask(m_Logger, repo.Object);

            var versionChangeLog_v1_0 = GetSingleVersionChangeLog("1.0.0", TestGitIds.Id2);
            var versionChangeLog_v1_1 = GetSingleVersionChangeLog("1.1.0", TestGitIds.Id7);
            var versionChangeLog_v1_2 = GetSingleVersionChangeLog("1.2.0", TestGitIds.Id4);

            var changelog = new ApplicationChangeLog()
            {
                versionChangeLog_v1_0, versionChangeLog_v1_1, versionChangeLog_v1_2
            };

            // ACT
            var result = await sut.RunAsync(changelog);

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Success, result);

            Assert.Collection(
                versionChangeLog_v1_0.AllCommits.Select(x => x.CommitMessage).Order(),
                msg => Assert.Equal(commit1.CommitMessage, msg),
                msg => Assert.Equal(commit2.CommitMessage, msg)
            );

            Assert.Collection(
                versionChangeLog_v1_1.AllCommits.Select(x => x.CommitMessage).Order(),
                msg => Assert.Equal(commit5.CommitMessage, msg),
                msg => Assert.Equal(commit6.CommitMessage, msg),
                msg => Assert.Equal(commit7.CommitMessage, msg)
            );

            Assert.Collection(
                versionChangeLog_v1_2.AllCommits.Select(x => x.CommitMessage).Order(),
                msg => Assert.Equal(commit3.CommitMessage, msg),
                msg => Assert.Equal(commit4.CommitMessage, msg)
            );
        }
    }
}
