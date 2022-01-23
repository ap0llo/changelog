using System;
using System.Threading.Tasks;
using Grynwald.ChangeLog.Configuration;
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
    /// Tests for <see cref="LoadMessageOverridesFromGitNotesTask"/>
    /// </summary>
    public class LoadMessageOverridesFromGitNotesTaskTest : TestBase
    {
        private readonly ILogger<LoadMessageOverridesFromGitNotesTask> m_Logger;


        public LoadMessageOverridesFromGitNotesTaskTest(ITestOutputHelper testOutputHelper)
        {
            m_Logger = new XunitLogger<LoadMessageOverridesFromGitNotesTask>(testOutputHelper);
        }


        [Fact]
        public void Logger_must_not_be_null()
        {
            // ARRANGE

            // ACT 
            var ex = Record.Exception(() => new LoadMessageOverridesFromGitNotesTask(logger: null!, configuration: new ChangeLogConfiguration(), repository: Mock.Of<IGitRepository>(MockBehavior.Strict)));

            // ASSERT
            var argumentNullException = Assert.IsType<ArgumentNullException>(ex);
            Assert.Equal("logger", argumentNullException.ParamName);
        }

        [Fact]
        public void Configuration_must_not_be_null()
        {
            // ARRANGE

            // ACT 
            var ex = Record.Exception(() => new LoadMessageOverridesFromGitNotesTask(logger: m_Logger, configuration: null!, repository: Mock.Of<IGitRepository>(MockBehavior.Strict)));

            // ASSERT
            var argumentNullException = Assert.IsType<ArgumentNullException>(ex);
            Assert.Equal("configuration", argumentNullException.ParamName);
        }

        [Fact]
        public void Repository_must_not_be_null()
        {
            // ARRANGE

            // ACT 
            var ex = Record.Exception(() => new LoadMessageOverridesFromGitNotesTask(logger: m_Logger, configuration: new ChangeLogConfiguration(), repository: null!));

            // ASSERT
            var argumentNullException = Assert.IsType<ArgumentNullException>(ex);
            Assert.Equal("repository", argumentNullException.ParamName);
        }

        [Fact]
        public async Task Run_does_nothing_for_empty_changelog()
        {
            // ARRANGE
            var repo = Mock.Of<IGitRepository>(MockBehavior.Strict);
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();

            var sut = new LoadMessageOverridesFromGitNotesTask(m_Logger, config, repo);

            // ACT
            var changelog = new ApplicationChangeLog();
            var result = await sut.RunAsync(changelog);

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Skipped, result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("changelog/message-overrides")]
        [InlineData("custom-namespace")]
        public async Task Run_replaces_commit_messages_with_messages_from_git_notes_a_git_not_exists(string? gitNotesNamespace)
        {
            // ARRANGE
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();

            if (gitNotesNamespace is not null)
                config.MessageOverrides.GitNotesNamespace = gitNotesNamespace;

            var commit1 = GetGitCommit(id: TestGitIds.Id1, commitMessage: "Original Message 1");
            var commit2 = GetGitCommit(id: TestGitIds.Id2, commitMessage: "Original Message 2");
            var versionChangeLog = GetSingleVersionChangeLog("1.2.3", TestGitIds.Id1);
            {
                versionChangeLog.Add(commit1);
                versionChangeLog.Add(commit2);
            }

            var repo = new Mock<IGitRepository>(MockBehavior.Strict);
            {
                repo.Setup(x => x.GetNotes(commit1.Id)).Returns(Array.Empty<GitNote>());

                repo
                    .Setup(x => x.GetNotes(commit2.Id))
                    .Returns(new[]
                    {
                        new GitNote(commit2.Id, "commits", "some text"),
                        new GitNote(commit2.Id, gitNotesNamespace ?? "changelog/message-overrides", "Overridden Message 2"),
                    });
            }

            var sut = new LoadMessageOverridesFromGitNotesTask(m_Logger, config, repo.Object);

            // ACT
            var result = await sut.RunAsync(new ApplicationChangeLog() { versionChangeLog });

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Success, result);
            Assert.Collection(
                versionChangeLog.AllCommits,
                c => Assert.Equal(commit1, c),
                c => Assert.Equal(commit2.WithCommitMessage("Overridden Message 2"), c)
            );
        }
    }
}
