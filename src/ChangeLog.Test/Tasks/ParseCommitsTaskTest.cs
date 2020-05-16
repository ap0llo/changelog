using System;
using System.Linq;
using System.Threading.Tasks;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.ConventionalCommits;
using Grynwald.ChangeLog.Git;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Grynwald.ChangeLog.Test.Tasks
{
    /// <summary>
    /// Unit tests for <see cref="ParseCommitsTask"/>
    /// </summary>
    public class ParseCommitsTaskTest : TestBase
    {
        private readonly ILogger<ParseCommitsTask> m_Logger = NullLogger<ParseCommitsTask>.Instance;
        private readonly ChangeLogConfiguration m_DefaultConfiguration = ChangeLogConfigurationLoader.GetDefaultConfiguration();

        [Fact]
        public async Task Run_does_nothing_for_empty_changelog()
        {
            // ARRANGE
            var repo = Mock.Of<IGitRepository>(MockBehavior.Strict);

            var sut = new ParseCommitsTask(m_Logger, m_DefaultConfiguration, repo);

            // ACT
            var changelog = new ApplicationChangeLog();
            var result = await sut.RunAsync(changelog);

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Skipped, result);
        }

        [Fact]
        public async Task Run_adds_all_parsable_changes_if_no_previous_version_exists()
        {
            // ARRANGE
            var repo = new Mock<IGitRepository>(MockBehavior.Strict);
            repo
                .Setup(x => x.GetCommits(null, It.IsAny<GitId>()))
                .Returns(new[]
                {
                    GetGitCommit("01", "feat: Some new feature"),
                    GetGitCommit("02", "fix: Some bugfix")
                });

            var sut = new ParseCommitsTask(m_Logger, m_DefaultConfiguration, repo.Object);

            var versionChangeLog = GetSingleVersionChangeLog("1.2.3", "01");
            var changelog = new ApplicationChangeLog() { versionChangeLog };

            // ACT
            var result = await sut.RunAsync(changelog);

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Success, result);

            repo.Verify(x => x.GetCommits(It.IsAny<GitId?>(), It.IsAny<GitId>()), Times.Once);

            Assert.NotNull(versionChangeLog.AllEntries);
            Assert.Equal(2, versionChangeLog.AllEntries.Count());

            {
                var entry = Assert.Single(versionChangeLog.AllEntries, e => e.Type.Equals(CommitType.Feature));
                Assert.Equal(new GitId("01"), entry.Commit);
                Assert.Null(entry.Scope);
                Assert.Equal("Some new feature", entry.Summary);
                Assert.NotNull(entry.Body);
                Assert.Empty(entry.Body);
            }
            {
                var entry = Assert.Single(versionChangeLog.AllEntries, e => e.Type.Equals(CommitType.BugFix));
                Assert.Equal(new GitId("02"), entry.Commit);
                Assert.Null(entry.Scope);
                Assert.Equal("Some bugfix", entry.Summary);
                Assert.NotNull(entry.Body);
                Assert.Empty(entry.Body);
            }
        }

        [Fact]
        public async Task Run_adds_the_expected_entries_if_a_previous_version_exists()
        {
            // ARRANGE
            var repo = new Mock<IGitRepository>(MockBehavior.Strict);
            repo
                .Setup(x => x.GetCommits(null, new GitId("01")))
                .Returns(Array.Empty<GitCommit>());

            repo
                .Setup(x => x.GetCommits(new GitId("01"), new GitId("02")))
                .Returns(new[]
                {
                    GetGitCommit("ab", "feat: Some new feature" ),
                    GetGitCommit("cd", "fix: Some bugfix" ),
                });

            var sut = new ParseCommitsTask(m_Logger, m_DefaultConfiguration, repo.Object);

            var versionChangeLog1 = GetSingleVersionChangeLog("1.2.3", "01");
            var versionChangeLog2 = GetSingleVersionChangeLog("2.4.5", "02");
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

            Assert.NotNull(versionChangeLog1.AllEntries);
            Assert.Empty(versionChangeLog1.AllEntries);

            Assert.NotNull(versionChangeLog2.AllEntries);
            Assert.Equal(2, versionChangeLog2.AllEntries.Count());

            {
                var entry = Assert.Single(versionChangeLog2.AllEntries, e => e.Type.Equals(CommitType.Feature));
                Assert.Equal(new GitId("ab"), entry.Commit);
                Assert.Null(entry.Scope);
                Assert.Equal("Some new feature", entry.Summary);
                Assert.NotNull(entry.Body);
                Assert.Empty(entry.Body);
            }
            {
                var entry = Assert.Single(versionChangeLog2.AllEntries, e => e.Type.Equals(CommitType.BugFix));
                Assert.Equal(new GitId("cd"), entry.Commit);
                Assert.Null(entry.Scope);
                Assert.Equal("Some bugfix", entry.Summary);
                Assert.NotNull(entry.Body);
                Assert.Empty(entry.Body);
            }
        }

        [Fact]
        public async Task Run_ignores_unparsable_commit_messages()
        {
            // ARRANGE
            var repo = new Mock<IGitRepository>(MockBehavior.Strict);
            repo
                .Setup(x => x.GetCommits(null, It.IsAny<GitId>()))
                .Returns(new[]
                {
                    GetGitCommit(commitMessage: "Not a conventional commit"),
                });

            var sut = new ParseCommitsTask(m_Logger, m_DefaultConfiguration, repo.Object);

            var versionChangeLog = GetSingleVersionChangeLog("1.2.3", "01");
            var changelog = new ApplicationChangeLog() { versionChangeLog };

            // ACT
            var result = await sut.RunAsync(changelog);

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Success, result);
            Assert.NotNull(versionChangeLog.AllEntries);
            Assert.Empty(versionChangeLog.AllEntries);
        }

        [Fact]
        public async Task Run_uses_configured_parser_setting_01()
        {
            // ARRANGE
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            config.Parser.Mode = ChangeLogConfiguration.ParserMode.Loose;

            var repo = new Mock<IGitRepository>(MockBehavior.Strict);
            repo
                .Setup(x => x.GetCommits(null, It.IsAny<GitId>()))
                .Returns(new[]
                {
                    // commit message is only parsable in "Loose" mode
                    GetGitCommit(commitMessage: "feat: Some Description\r\n" + "\r\n" + "\r\n" +  "Message Body\r\n"),
                });

            var sut = new ParseCommitsTask(m_Logger, config, repo.Object);

            var versionChangeLog = GetSingleVersionChangeLog("1.2.3", "01");
            var changelog = new ApplicationChangeLog() { versionChangeLog };

            // ACT
            var result = await sut.RunAsync(changelog);

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Success, result);
            Assert.NotNull(versionChangeLog.AllEntries);
            Assert.Single(versionChangeLog.AllEntries);
        }

        [Fact]
        public async Task Run_uses_configured_parser_setting_02()
        {
            // ARRANGE
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            config.Parser.Mode = ChangeLogConfiguration.ParserMode.Strict;

            var repo = new Mock<IGitRepository>(MockBehavior.Strict);
            repo
                .Setup(x => x.GetCommits(null, It.IsAny<GitId>()))
                .Returns(new[]
                {
                    // commit message is only parsable in "Loose" mode
                    GetGitCommit(commitMessage: "feat: Some Description\r\n" + "\r\n" + "\r\n" +  "Message Body\r\n"),
                });

            var sut = new ParseCommitsTask(m_Logger, config, repo.Object);

            var versionChangeLog = GetSingleVersionChangeLog("1.2.3", "01");
            var changelog = new ApplicationChangeLog() { versionChangeLog };

            // ACT
            var result = await sut.RunAsync(changelog);

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Success, result);
            Assert.NotNull(versionChangeLog.AllEntries);
            Assert.Empty(versionChangeLog.AllEntries);
        }

        //TODO: Scope, footers, body
    }
}
