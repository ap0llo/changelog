using System;
using System.Threading.Tasks;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.Git;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Pipeline;
using Grynwald.ChangeLog.Tasks;
using Grynwald.Utilities.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace Grynwald.ChangeLog.Test.Tasks
{
    /// <summary>
    /// Tests for <see cref="LoadMessageOverridesFromFileSystemTask"/>
    /// </summary>
    public class LoadMessageOverridesFromFileSystemTaskTest : TestBase
    {
        private readonly ILogger<LoadMessageOverridesFromFileSystemTask> m_Logger;


        public LoadMessageOverridesFromFileSystemTaskTest(ITestOutputHelper testOutputHelper)
        {
            m_Logger = new XunitLogger<LoadMessageOverridesFromFileSystemTask>(testOutputHelper);
        }


        [Fact]
        public void Logger_must_not_be_null()
        {
            // ARRANGE

            // ACT 
            var ex = Record.Exception(() => new LoadMessageOverridesFromFileSystemTask(logger: null!, configuration: new(), repository: Mock.Of<IGitRepository>(MockBehavior.Strict)));

            // ASSERT
            var argumentNullException = Assert.IsType<ArgumentNullException>(ex);
            Assert.Equal("logger", argumentNullException.ParamName);
        }

        [Fact]
        public void Configuration_must_not_be_null()
        {
            // ARRANGE

            // ACT 
            var ex = Record.Exception(() => new LoadMessageOverridesFromFileSystemTask(logger: m_Logger, configuration: null!, repository: Mock.Of<IGitRepository>(MockBehavior.Strict)));

            // ASSERT
            var argumentNullException = Assert.IsType<ArgumentNullException>(ex);
            Assert.Equal("configuration", argumentNullException.ParamName);
        }

        [Fact]
        public void Repository_must_not_be_null()
        {
            // ARRANGE

            // ACT 
            var ex = Record.Exception(() => new LoadMessageOverridesFromFileSystemTask(logger: m_Logger, configuration: new(), repository: null!));

            // ASSERT
            var argumentNullException = Assert.IsType<ArgumentNullException>(ex);
            Assert.Equal("repository", argumentNullException.ParamName);
        }

        [Fact]
        public async Task Task_does_nothing_for_empty_changelog()
        {
            // ARRANGE
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            var repo = new Mock<IGitRepository>(MockBehavior.Strict);

            var sut = new LoadMessageOverridesFromFileSystemTask(m_Logger, config, repo.Object);

            // ACT
            var changelog = new ApplicationChangeLog();
            var result = await sut.RunAsync(changelog);

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Skipped, result);
        }

        [Theory]
        [InlineData("X:\\Does-not-exist")]
        [InlineData("/Does-not-exist")]
        [InlineData("does-not-exist")]
        public async Task Task_succeeds_if_override_directory_does_not_exist(string sourceDirectoryPath)
        {
            // ARRANGE
            using var repositoryDirectory = new TemporaryDirectory();

            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            {
                config.RepositoryPath = repositoryDirectory;
                config.MessageOverrides.SourceDirectoryPath = sourceDirectoryPath;
            }

            var commit1 = GetGitCommit(id: TestGitIds.Id1, commitMessage: "Original Message 1");
            var commit2 = GetGitCommit(id: TestGitIds.Id2, commitMessage: "Original Message 2");

            var repo = new Mock<IGitRepository>(MockBehavior.Strict);

            var versionChangeLog = GetSingleVersionChangeLog("1.2.3", TestGitIds.Id1);
            {
                versionChangeLog.Add(commit1);
                versionChangeLog.Add(commit2);
            }

            var sut = new LoadMessageOverridesFromFileSystemTask(m_Logger, config, repo.Object);

            // ACT
            var result = await sut.RunAsync(new ApplicationChangeLog() { versionChangeLog });

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Success, result);
            Assert.Collection(
                versionChangeLog.AllCommits,
                c => Assert.Equal(commit1, c),
                c => Assert.Equal(commit2, c)
            );
        }

        [Theory]
        [InlineData(null)]
        [InlineData(".config/changelog/message-overrides")]
        [InlineData("custom/directory")]
        public async Task Task_replaces_commit_messages_with_messages_from_the_configured_directory(string? sourceDirectoryPath)
        {
            // ARRANGE
            using var repositoryDirectory = new TemporaryDirectory();

            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            {
                config.RepositoryPath = repositoryDirectory;

                if (sourceDirectoryPath is not null)
                    config.MessageOverrides.SourceDirectoryPath = sourceDirectoryPath;
            }


            var commit1 = GetGitCommit(id: TestGitIds.Id1, commitMessage: "Original Message 1");
            var commit2 = GetGitCommit(id: TestGitIds.Id2, commitMessage: "Original Message 2");

            var repo = new Mock<IGitRepository>(MockBehavior.Strict);
            {
                repo.SetupTryGetCommit();
                repo.SetupTryGetCommit(commit1);
                repo.SetupTryGetCommit(commit2);
            }

            var versionChangeLog = GetSingleVersionChangeLog("1.2.3", TestGitIds.Id1);
            {
                versionChangeLog.Add(commit1);
                versionChangeLog.Add(commit2);
            }

            repositoryDirectory.AddFile(
                $"{sourceDirectoryPath ?? ".config/changelog/message-overrides"}/{commit2.Id.ToString(abbreviate: false)}",
                "Overridden Message 2");

            var sut = new LoadMessageOverridesFromFileSystemTask(m_Logger, config, repo.Object);

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

        [Fact]
        public async Task Task_replaces_commit_messages_with_messages_from_the_directory_configured_using_an_absolute_path()
        {
            // ARRANGE
            using var repositoryDirectory = new TemporaryDirectory();
            using var overridesDirectory = new TemporaryDirectory();

            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            {
                config.RepositoryPath = repositoryDirectory;
                config.MessageOverrides.SourceDirectoryPath = overridesDirectory;
            }


            var commit1 = GetGitCommit(id: TestGitIds.Id1, commitMessage: "Original Message 1");
            var commit2 = GetGitCommit(id: TestGitIds.Id2, commitMessage: "Original Message 2");

            var repo = new Mock<IGitRepository>(MockBehavior.Strict);
            {
                repo.SetupTryGetCommit();
                repo.SetupTryGetCommit(commit1);
                repo.SetupTryGetCommit(commit2);
            }

            var versionChangeLog = GetSingleVersionChangeLog("1.2.3", TestGitIds.Id1);
            {
                versionChangeLog.Add(commit1);
                versionChangeLog.Add(commit2);
            }

            overridesDirectory.AddFile(commit2.Id.ToString(), "Overridden Message 2");

            var sut = new LoadMessageOverridesFromFileSystemTask(m_Logger, config, repo.Object);

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

        [Fact]
        public async Task Task_replaces_commit_messages_if_file_name_is_abbreviated_id()
        {
            // ARRANGE
            using var temporaryDirectory = new TemporaryDirectory();

            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            {
                config.RepositoryPath = temporaryDirectory;
            }

            var commit1 = GetGitCommit(id: TestGitIds.Id1, commitMessage: "Original Message 1");
            var commit2 = GetGitCommit(id: TestGitIds.Id2, commitMessage: "Original Message 2");

            var repo = new Mock<IGitRepository>(MockBehavior.Strict);
            {
                repo.SetupTryGetCommit();
                repo.SetupTryGetCommit(commit1);
                repo.SetupTryGetCommit(commit2);
            }

            var versionChangeLog = GetSingleVersionChangeLog("1.2.3", TestGitIds.Id1);
            {
                versionChangeLog.Add(commit1);
                versionChangeLog.Add(commit2);
            }

            temporaryDirectory.AddFile(
                $".config/changelog/message-overrides/{commit2.Id.ToString(abbreviate: true)}",
                "Overridden Message 2");


            var sut = new LoadMessageOverridesFromFileSystemTask(m_Logger, config, repo.Object);

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

        [Fact]
        public async Task Task_fails_if_multiple_files_in_the_override_directory_resolve_to_the_same_commit()
        {
            // ARRANGE
            using var overrideDirectory = new TemporaryDirectory();

            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            {
                config.MessageOverrides.SourceDirectoryPath = overrideDirectory;
            }

            var commit1 = GetGitCommit(id: TestGitIds.Id1, commitMessage: "Original Message 1");
            var commit2 = GetGitCommit(id: TestGitIds.Id2, commitMessage: "Original Message 2");

            var repo = new Mock<IGitRepository>(MockBehavior.Strict);
            {
                repo.SetupTryGetCommit();
                repo.SetupTryGetCommit(commit1);
                repo.SetupTryGetCommit(commit2);
            }

            var versionChangeLog = GetSingleVersionChangeLog("1.2.3", TestGitIds.Id1);
            {
                versionChangeLog.Add(commit1);
                versionChangeLog.Add(commit2);
            }

            overrideDirectory.AddFile(commit2.Id.ToString(abbreviate: true), "Overridden Message 2.1");
            overrideDirectory.AddFile(commit2.Id.ToString(abbreviate: false), "Overridden Message 2.2");

            var sut = new LoadMessageOverridesFromFileSystemTask(m_Logger, config, repo.Object);

            // ACT
            var result = await sut.RunAsync(new ApplicationChangeLog() { versionChangeLog });

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Error, result);
            Assert.Collection(
                versionChangeLog.AllCommits,
                c => Assert.Equal(commit1, c),
                c => Assert.Equal(commit2, c)
            );
        }

        [Fact]
        public async Task Task_ignores_files_in_the_override_directory_if_the_commit_cannot_be_resolved()
        {
            // ARRANGE
            using var overrideDirectory = new TemporaryDirectory();

            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            {
                config.MessageOverrides.SourceDirectoryPath = overrideDirectory;
            }

            var commit1 = GetGitCommit(id: TestGitIds.Id1, commitMessage: "Original Message 1");
            var commit2 = GetGitCommit(id: TestGitIds.Id2, commitMessage: "Original Message 2");

            var repo = new Mock<IGitRepository>(MockBehavior.Strict);
            {
                repo.SetupTryGetCommit();
                repo.SetupTryGetCommit(commit1);
                repo.SetupTryGetCommit(commit2);
            }

            var versionChangeLog = GetSingleVersionChangeLog("1.2.3", TestGitIds.Id1);
            {
                versionChangeLog.Add(commit1);
                versionChangeLog.Add(commit2);
            }

            overrideDirectory.AddFile("not-a-commit-id", "Some content");

            var sut = new LoadMessageOverridesFromFileSystemTask(m_Logger, config, repo.Object);

            // ACT
            var result = await sut.RunAsync(new ApplicationChangeLog() { versionChangeLog });

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Success, result);
            Assert.Collection(
                versionChangeLog.AllCommits,
                c => Assert.Equal(commit1, c),
                c => Assert.Equal(commit2, c)
            );
        }
    }
}
