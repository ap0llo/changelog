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
    /// Unit tests for <see cref="LoadVersionsFromTagsTask"/>.
    /// </summary>
    public class LoadVersionsFromTagsTaskTest
    {
        private readonly ILogger<LoadVersionsFromTagsTask> m_Logger;


        public LoadVersionsFromTagsTaskTest(ITestOutputHelper testOutputHelper)
        {
            m_Logger = new XunitLogger<LoadVersionsFromTagsTask>(testOutputHelper);
        }


        [Fact]
        public async Task Run_adds_versions_from_tags()
        {
            // ARRANGE
            var tags = new GitTag[]
            {
                new GitTag("1.2.3-alpha", TestGitIds.Id1),
                new GitTag("4.5.6", TestGitIds.Id2)
            };

            var repoMock = new Mock<IGitRepository>(MockBehavior.Strict);
            repoMock.Setup(x => x.GetTags()).Returns(tags);

            var sut = new LoadVersionsFromTagsTask(m_Logger, ChangeLogConfigurationLoader.GetDefaultConfiguration(), repoMock.Object);

            // ACT
            var changeLog = new ApplicationChangeLog();
            var result = await sut.RunAsync(changeLog);

            // ASSERT
            Assert.All(
                tags,
                tag =>
                {
                    var version = NuGetVersion.Parse(tag.Name);
                    Assert.Contains(new VersionInfo(version, tag.Commit), changeLog.Versions);
                });
            Assert.Equal(ChangeLogTaskResult.Success, result);
        }

        [Fact]
        public async Task Run_does_not_add_any_versions_if_no_tag_patterns_are_specified()
        {
            // ARRANGE
            var tags = new GitTag[]
            {
                new GitTag("1.2.3-alpha", TestGitIds.Id1),
                new GitTag("4.5.6", TestGitIds.Id2)
            };

            var repoMock = new Mock<IGitRepository>(MockBehavior.Strict);
            repoMock.Setup(x => x.GetTags()).Returns(tags);

            var config = new ChangeLogConfiguration() { TagPatterns = Array.Empty<string>() };

            var sut = new LoadVersionsFromTagsTask(m_Logger, config, repoMock.Object);

            // ACT
            var changeLog = new ApplicationChangeLog();
            var result = await sut.RunAsync(changeLog);

            // ASSERT
            Assert.NotNull(changeLog.Versions);
            Assert.Empty(changeLog.Versions);
            Assert.Equal(ChangeLogTaskResult.Skipped, result);
        }

        [Theory]
        [InlineData("not-a-version")]
        [InlineData("1.2.3.4.5")]
        public async Task Run_ignores_tags_that_are_not_a_valid_version(string tagName)
        {
            // ARRANGE
            var tags = new GitTag[]
            {
                new GitTag(tagName, TestGitIds.Id1),
            };

            var repoMock = new Mock<IGitRepository>(MockBehavior.Strict);
            repoMock.Setup(x => x.GetTags()).Returns(tags);

            var sut = new LoadVersionsFromTagsTask(m_Logger, ChangeLogConfigurationLoader.GetDefaultConfiguration(), repoMock.Object);

            // ACT
            var changeLog = new ApplicationChangeLog();
            var result = await sut.RunAsync(changeLog);

            // ASSERT
            Assert.Empty(changeLog.Versions);
            Assert.Equal(ChangeLogTaskResult.Success, result);
        }

        [Theory]
        [InlineData("1.0", "1.0.0")]
        [InlineData("1.0.0", "1.0.0")]
        [InlineData("1.0-alpha", "1.0.0-alpha")]
        [InlineData("1.0.0-beta", "1.0.0-beta")]
        [InlineData("1.2.3-alpha", "1.2.3-alpha")]
        [InlineData("1.2.3", "1.2.3")]
        // Tags may be prefixed with "v"
        [InlineData("v1.0", "1.0.0")]
        [InlineData("V1.0", "1.0.0")]
        [InlineData("v1.0.0", "1.0.0")]
        [InlineData("V1.0.0", "1.0.0")]
        [InlineData("v1.0-alpha", "1.0.0-alpha")]
        [InlineData("V1.0-alpha", "1.0.0-alpha")]
        [InlineData("v1.0.0-beta", "1.0.0-beta")]
        [InlineData("V1.0.0-beta", "1.0.0-beta")]
        [InlineData("v1.2.3-alpha", "1.2.3-alpha")]
        [InlineData("v4.5.6", "4.5.6")]
        [InlineData("V1.2.3-alpha", "1.2.3-alpha")]
        [InlineData("V4.5.6", "4.5.6")]
        public async Task Run_correctly_gets_version_from_tag_name_using_default_configuration(string tagName, string version)
        {
            // ARRANGE
            var tags = new GitTag[]
            {
                new GitTag(tagName, TestGitIds.Id1),
            };

            var repoMock = new Mock<IGitRepository>(MockBehavior.Strict);
            repoMock.Setup(x => x.GetTags()).Returns(tags);

            var expectedVersion = NuGetVersion.Parse(version);

            var sut = new LoadVersionsFromTagsTask(m_Logger, ChangeLogConfigurationLoader.GetDefaultConfiguration(), repoMock.Object);

            // ACT
            var changeLog = new ApplicationChangeLog();
            var result = await sut.RunAsync(changeLog);

            // ASSERT
            var versionInfo = Assert.Single(changeLog.Versions);
            Assert.Equal(expectedVersion, versionInfo.Version);
            Assert.Equal(ChangeLogTaskResult.Success, result);
        }

        [Fact]
        public async Task Run_ignores_duplicate_versions_from_tags()
        {
            // ARRANGE            
            var tags = new GitTag[]
            {
                new GitTag("v1.2.3", TestGitIds.Id1),
                new GitTag("4.5.6", TestGitIds.Id2),
                new GitTag("1.2.3", TestGitIds.Id3)
            };

            var repoMock = new Mock<IGitRepository>(MockBehavior.Strict);
            repoMock.Setup(x => x.GetTags()).Returns(tags);

            var sut = new LoadVersionsFromTagsTask(m_Logger, ChangeLogConfigurationLoader.GetDefaultConfiguration(), repoMock.Object);

            // ACT
            var changeLog = new ApplicationChangeLog();
            var result = await sut.RunAsync(changeLog);

            // ASSERT
            Assert.Contains(changeLog.Versions, x => x.Version == NuGetVersion.Parse("1.2.3") && x.Commit == TestGitIds.Id1);
            Assert.DoesNotContain(changeLog.Versions, x => x.Version == NuGetVersion.Parse("1.2.3") && x.Commit == TestGitIds.Id3);
            Assert.Equal(ChangeLogTaskResult.Success, result);
        }


        [Fact]
        public async Task Task_fails_if_version_was_already_added_by_a_previous_task()
        {
            // ARRANGE            
            var tags = new GitTag[]
            {
                new GitTag("1.2.3", TestGitIds.Id1),
            };

            var repoMock = new Mock<IGitRepository>(MockBehavior.Strict);
            repoMock.Setup(x => x.GetTags()).Returns(tags);

            var sut = new LoadVersionsFromTagsTask(m_Logger, ChangeLogConfigurationLoader.GetDefaultConfiguration(), repoMock.Object);

            var changeLog = new ApplicationChangeLog()
            {
                new SingleVersionChangeLog(new VersionInfo(NuGetVersion.Parse("1.2.3"), TestGitIds.Id2))
            };

            // ACT
            var result = await sut.RunAsync(changeLog);

            // ASSERT            
            Assert.Equal(ChangeLogTaskResult.Error, result);
            Assert.DoesNotContain(changeLog.Versions, x => x.Version == NuGetVersion.Parse("1.2.3") && x.Commit == TestGitIds.Id1);
        }

    }
}
