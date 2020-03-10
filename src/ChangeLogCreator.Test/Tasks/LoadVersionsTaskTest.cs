using System;
using System.Threading.Tasks;
using ChangeLogCreator.Configuration;
using ChangeLogCreator.Git;
using ChangeLogCreator.Model;
using ChangeLogCreator.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NuGet.Versioning;
using Xunit;

namespace ChangeLogCreator.Test.Tasks
{
    /// <summary>
    /// Unit tests for <see cref="LoadVersionsTask"/>.
    /// </summary>
    public class LoadVersionsTaskTest
    {
        private readonly ILogger<LoadVersionsTask> m_Logger = NullLogger<LoadVersionsTask>.Instance;

        [Fact]
        public async Task Run_adds_versions_from_tags()
        {
            // ARRANGE
            var tags = new GitTag[]
            {
                new GitTag("1.2.3-alpha", new GitId("01")),
                new GitTag("4.5.6", new GitId("02"))
            };

            var repoMock = new Mock<IGitRepository>(MockBehavior.Strict);
            repoMock.Setup(x => x.GetTags()).Returns(tags);

            var sut = new LoadVersionsTask(m_Logger, ChangeLogConfigurationLoader.GetDefaultConfiguration(), repoMock.Object);

            // ACT
            var changeLog = new ChangeLog();
            await sut.RunAsync(changeLog);

            // ASSERT
            Assert.All(
                tags,
                tag =>
                {
                    var version = NuGetVersion.Parse(tag.Name);
                    Assert.Contains(new VersionInfo(version, tag.Commit), changeLog.Versions);
                });
        }

        [Fact]
        public async Task Run_does_not_add_any_versions_if_no_tag_patterns_are_specified()
        {
            // ARRANGE
            var tags = new GitTag[]
            {
                new GitTag("1.2.3-alpha", new GitId("01")),
                new GitTag("4.5.6", new GitId("02"))
            };

            var repoMock = new Mock<IGitRepository>(MockBehavior.Strict);
            repoMock.Setup(x => x.GetTags()).Returns(tags);

            var config = new ChangeLogConfiguration() { TagPatterns = Array.Empty<string>() };

            var sut = new LoadVersionsTask(m_Logger, config, repoMock.Object);

            // ACT
            var changeLog = new ChangeLog();
            await sut.RunAsync(changeLog);

            // ASSERT
            Assert.NotNull(changeLog.Versions);
            Assert.Empty(changeLog.Versions);
        }

        [Theory]
        [InlineData("not-a-version")]
        [InlineData("1.2.3.4.5")]
        public async Task Run_ignores_tags_that_are_not_a_valid_version(string tagName)
        {
            // ARRANGE
            var tags = new GitTag[]
            {
                new GitTag(tagName, new GitId("01")),
            };

            var repoMock = new Mock<IGitRepository>(MockBehavior.Strict);
            repoMock.Setup(x => x.GetTags()).Returns(tags);

            var sut = new LoadVersionsTask(m_Logger, ChangeLogConfigurationLoader.GetDefaultConfiguration(), repoMock.Object);

            // ACT
            var changeLog = new ChangeLog();
            await sut.RunAsync(changeLog);

            // ASSERT
            Assert.Empty(changeLog.Versions);
        }

        [Theory]
        [InlineData("1.2.3-alpha", "1.2.3-alpha")]
        [InlineData("1.2.3", "1.2.3")]
        // Tags may be prefixed with "v"
        [InlineData("v1.2.3-alpha", "1.2.3-alpha")]
        [InlineData("v4.5.6", "4.5.6")]
        [InlineData("V1.2.3-alpha", "1.2.3-alpha")]
        [InlineData("V4.5.6", "4.5.6")]
        public async Task Run_correctly_gets_version_from_tag_name_using_default_configuration(string tagName, string version)
        {
            // ARRANGE
            var tags = new GitTag[]
            {
                new GitTag(tagName, new GitId("0123")),
            };

            var repoMock = new Mock<IGitRepository>(MockBehavior.Strict);
            repoMock.Setup(x => x.GetTags()).Returns(tags);

            var expectedVersion = SemanticVersion.Parse(version);

            var sut = new LoadVersionsTask(m_Logger, ChangeLogConfigurationLoader.GetDefaultConfiguration(), repoMock.Object);

            // ACT
            var changeLog = new ChangeLog();
            await sut.RunAsync(changeLog);

            // ASSERT
            var versionInfo = Assert.Single(changeLog.Versions);
            Assert.Equal(expectedVersion, versionInfo.Version);
        }
    }
}
