using System;
using System.Collections.Generic;
using Grynwald.ChangeLog.Integrations.GitHub;
using Xunit;

namespace Grynwald.ChangeLog.Test.Integrations.GitHub
{
    /// <summary>
    /// Tests for <see cref="GitHubProjectInfo"/>
    /// </summary>
    public class GitHubProjectInfoTest : EqualityTest<GitHubProjectInfo, GitHubProjectInfoTest>, IEqualityTestDataProvider<GitHubProjectInfo>
    {
        public IEnumerable<(GitHubProjectInfo left, GitHubProjectInfo right)> GetEqualTestCases()
        {
            yield return (
                new GitHubProjectInfo("example.com", "user", "repo"),
                new GitHubProjectInfo("example.com", "user", "repo")
            );

            // Comparisons must be case-insensitive
            yield return (
                new GitHubProjectInfo("example.com", "user", "repo"),
                new GitHubProjectInfo("EXAMPLE.COM", "user", "repo")
            );
            yield return (
                new GitHubProjectInfo("example.com", "user", "repo"),
                new GitHubProjectInfo("example.com", "USER", "repo")
            );
            yield return (
                new GitHubProjectInfo("example.com", "user", "repo"),
                new GitHubProjectInfo("example.com", "user", "REPO")
            );
        }

        public IEnumerable<(GitHubProjectInfo left, GitHubProjectInfo right)> GetUnequalTestCases()
        {
            yield return (
                new GitHubProjectInfo("example.com", "user", "repo"),
                new GitHubProjectInfo("example.net", "user", "repo")
            );
            yield return (
                new GitHubProjectInfo("example.com", "user1", "repo"),
                new GitHubProjectInfo("example.com", "user2", "repo")
            );
            yield return (
                new GitHubProjectInfo("example.com", "user", "repo1"),
                new GitHubProjectInfo("example.com", "user", "repo2")
            );
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("\t")]
        public void Host_must_not_be_null_or_whitespace(string? host)
        {
            Assert.Throws<ArgumentException>(() => new GitHubProjectInfo(host!, "user", "repo"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("\t")]
        public void User_must_not_be_null_or_whitespace(string? user)
        {
            Assert.Throws<ArgumentException>(() => new GitHubProjectInfo("example.com", user!, "repo"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("\t")]
        public void Project_must_not_be_null_or_whitespace(string? project)
        {
            Assert.Throws<ArgumentException>(() => new GitHubProjectInfo("example.com", "user", project!));
        }
    }
}
