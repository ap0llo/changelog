using System;
using System.Linq;
using System.Threading.Tasks;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.ConventionalCommits;
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
    /// Unit tests for <see cref="ParseCommitsTask"/>
    /// </summary>
    public class ParseCommitsTaskTest : TestBase
    {
        private readonly ILogger<ParseCommitsTask> m_Logger;
        private readonly ChangeLogConfiguration m_DefaultConfiguration;


        public ParseCommitsTaskTest(ITestOutputHelper testOutputHelper)
        {
            m_Logger = new XunitLogger<ParseCommitsTask>(testOutputHelper);
            m_DefaultConfiguration = ChangeLogConfigurationLoader.GetDefaultConfiguration();
        }


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
                    GetGitCommit(TestGitIds.Id1, "feat: Some new feature"),
                    GetGitCommit(TestGitIds.Id2, "fix: Some bugfix")
                });

            repo.Setup(x => x.GetNotes(It.IsAny<GitId>())).Returns(Array.Empty<GitNote>());


            var sut = new ParseCommitsTask(m_Logger, m_DefaultConfiguration, repo.Object);

            var versionChangeLog = GetSingleVersionChangeLog("1.2.3", TestGitIds.Id1);
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
                Assert.Equal(TestGitIds.Id1, entry.Commit);
                Assert.Null(entry.Scope);
                Assert.Equal("Some new feature", entry.Summary);
                Assert.NotNull(entry.Body);
                Assert.Empty(entry.Body);
            }
            {
                var entry = Assert.Single(versionChangeLog.AllEntries, e => e.Type.Equals(CommitType.BugFix));
                Assert.Equal(TestGitIds.Id2, entry.Commit);
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
                .Setup(x => x.GetCommits(null, TestGitIds.Id1))
                .Returns(Array.Empty<GitCommit>());

            repo
                .Setup(x => x.GetCommits(TestGitIds.Id1, TestGitIds.Id2))
                .Returns(new[]
                {
                    GetGitCommit(TestGitIds.Id1, "feat: Some new feature" ),
                    GetGitCommit(TestGitIds.Id2, "fix: Some bugfix" ),
                });

            repo.Setup(x => x.GetNotes(It.IsAny<GitId>())).Returns(Array.Empty<GitNote>());

            var sut = new ParseCommitsTask(m_Logger, m_DefaultConfiguration, repo.Object);

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

            Assert.NotNull(versionChangeLog1.AllEntries);
            Assert.Empty(versionChangeLog1.AllEntries);

            Assert.NotNull(versionChangeLog2.AllEntries);
            Assert.Equal(2, versionChangeLog2.AllEntries.Count());

            {
                var entry = Assert.Single(versionChangeLog2.AllEntries, e => e.Type.Equals(CommitType.Feature));
                Assert.Equal(TestGitIds.Id1, entry.Commit);
                Assert.Null(entry.Scope);
                Assert.Equal("Some new feature", entry.Summary);
                Assert.NotNull(entry.Body);
                Assert.Empty(entry.Body);
            }
            {
                var entry = Assert.Single(versionChangeLog2.AllEntries, e => e.Type.Equals(CommitType.BugFix));
                Assert.Equal(TestGitIds.Id2, entry.Commit);
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

            repo.Setup(x => x.GetNotes(It.IsAny<GitId>())).Returns(Array.Empty<GitNote>());

            var sut = new ParseCommitsTask(m_Logger, m_DefaultConfiguration, repo.Object);

            var versionChangeLog = GetSingleVersionChangeLog("1.2.3", TestGitIds.Id1);
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

            repo.Setup(x => x.GetNotes(It.IsAny<GitId>())).Returns(Array.Empty<GitNote>());

            var sut = new ParseCommitsTask(m_Logger, config, repo.Object);

            var versionChangeLog = GetSingleVersionChangeLog("1.2.3", TestGitIds.Id1);
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

            repo.Setup(x => x.GetNotes(It.IsAny<GitId>())).Returns(Array.Empty<GitNote>());

            var sut = new ParseCommitsTask(m_Logger, config, repo.Object);

            var versionChangeLog = GetSingleVersionChangeLog("1.2.3", TestGitIds.Id1);
            var changelog = new ApplicationChangeLog() { versionChangeLog };

            // ACT
            var result = await sut.RunAsync(changelog);

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Success, result);
            Assert.NotNull(versionChangeLog.AllEntries);
            Assert.Empty(versionChangeLog.AllEntries);
        }

        [Theory]
        [InlineData(true, null)]
        [InlineData(false, null)]
        [InlineData(null, null)]
        [InlineData(true, "changelog/message-overrides")]
        [InlineData(false, "changelog/message-overrides")]
        [InlineData(null, "changelog/message-overrides")]
        [InlineData(true, "custom-namespace")]
        [InlineData(false, "custom-namespace")]
        [InlineData(null, "custom-namespace")]
        public async Task Run_uses_override_messages_from_git_notes_if_they_exist_and_message_overrides_are_enabled(bool? enableMessageOverrides, string? gitNotesNamespace)
        {
            // ARRANGE
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();

            if (enableMessageOverrides.HasValue)
                config.MessageOverrides.Enabled = enableMessageOverrides.Value;

            if (gitNotesNamespace is not null)
                config.MessageOverrides.GitNotesNamespace = gitNotesNamespace;

            var shouldOverrideMessage = enableMessageOverrides == null || enableMessageOverrides == true;

            var repo = new Mock<IGitRepository>(MockBehavior.Strict);
            repo
                .Setup(x => x.GetCommits(null, It.IsAny<GitId>()))
                .Returns(new[]
                {
                    GetGitCommit(id: TestGitIds.Id1, commitMessage: "feat: Some Description"),
                    GetGitCommit(id: TestGitIds.Id2, commitMessage: "fix: Original Description"),
                });

            repo.Setup(x => x.GetNotes(TestGitIds.Id1)).Returns(Array.Empty<GitNote>());

            repo
                .Setup(x => x.GetNotes(TestGitIds.Id2))
                .Returns(new[]
                {
                    new GitNote(TestGitIds.Id2, "commits", "some text"),
                    new GitNote(TestGitIds.Id2, gitNotesNamespace ?? "changelog/message-overrides", "feat: New Description"),
                });


            var sut = new ParseCommitsTask(m_Logger, config, repo.Object);

            var versionChangeLog = GetSingleVersionChangeLog("1.2.3", TestGitIds.Id1);
            var changelog = new ApplicationChangeLog() { versionChangeLog };

            // ACT
            var result = await sut.RunAsync(changelog);

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Success, result);
            Assert.Collection(
                versionChangeLog.AllEntries,
                entry =>
                {
                    Assert.Equal(TestGitIds.Id1, entry.Commit);
                    Assert.Equal(CommitType.Feature, entry.Type);
                    Assert.Equal("Some Description", entry.Summary);
                },
                entry =>
                {
                    Assert.Equal(TestGitIds.Id2, entry.Commit);
                    if (shouldOverrideMessage)
                    {
                        Assert.Equal(CommitType.Feature, entry.Type);
                        Assert.Equal("New Description", entry.Summary);
                    }
                    else
                    {
                        Assert.Equal(CommitType.BugFix, entry.Type);
                        Assert.Equal("Original Description", entry.Summary);
                    }
                });
        }

        [Fact]
        public async Task Run_ignores_entry_if_override_message_cannot_be_parsed()
        {
            // ARRANGE
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();

            var repo = new Mock<IGitRepository>(MockBehavior.Strict);
            repo
                .Setup(x => x.GetCommits(null, It.IsAny<GitId>()))
                .Returns(new[]
                {
                    GetGitCommit(id: TestGitIds.Id1, commitMessage: "feat: Some Description"),
                    GetGitCommit(id: TestGitIds.Id2, commitMessage: "fix: Original Description"),
                });

            repo.Setup(x => x.GetNotes(TestGitIds.Id1)).Returns(Array.Empty<GitNote>());

            repo
                .Setup(x => x.GetNotes(TestGitIds.Id2))
                .Returns(new[]
                {
                    new GitNote(TestGitIds.Id2, "changelog/message-overrides", "Not a conventioal commit message"),
                });


            var sut = new ParseCommitsTask(m_Logger, config, repo.Object);

            var versionChangeLog = GetSingleVersionChangeLog("1.2.3", TestGitIds.Id1);
            var changelog = new ApplicationChangeLog() { versionChangeLog };

            // ACT
            var result = await sut.RunAsync(changelog);

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Success, result);
            Assert.Collection(
                versionChangeLog.AllEntries,
                entry =>
                {
                    Assert.Equal(TestGitIds.Id1, entry.Commit);
                    Assert.Equal(CommitType.Feature, entry.Type);
                    Assert.Equal("Some Description", entry.Summary);
                });
        }

        //TODO: Scope, footers, body

    }
}
