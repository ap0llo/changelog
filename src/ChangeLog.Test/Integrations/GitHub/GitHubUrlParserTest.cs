using System;
using System.Diagnostics;
using Grynwald.ChangeLog.Integrations.GitHub;
using Xunit;

namespace Grynwald.ChangeLog.Test.Integrations.GitHub
{
    /// <summary>
    /// Tests for <see cref="GitHubUrlParser"/>
    /// </summary>
    public class GitHubUrlParserTest
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("\t")]
        [InlineData("  ")]
        [InlineData("not-a-url")]
        [InlineData("http://github.com/owner/another-name/repo.git")] // to many segments in the path
        [InlineData("ftp://github.com/owner/repo.git")] // unsupported scheme
        public void ParseRemoteUrl_throws_ArgumentException_for_invalid_input(string? url)
        {
            Assert.ThrowsAny<ArgumentException>(() => GitHubUrlParser.ParseRemoteUrl(url!));
        }

        [Theory]
        [InlineData("http://github.com/owner/repo-name.git", "github.com", "owner", "repo-name")]
        [InlineData("https://github.com/owner/repo-name.git", "github.com", "owner", "repo-name")]
        [InlineData("git@github.com:owner/repo-name.git", "github.com", "owner", "repo-name")]
        public void ParseRemoteUrl_returns_the_expected_GitHubProjectInfo(string url, string host, string owner, string repository)
        {
            // ARRANGE

            // ACT 
            var projectInfo = GitHubUrlParser.ParseRemoteUrl(url);

            // ASSERT
            Assert.NotNull(projectInfo);
            Assert.Equal(host, projectInfo.Host);
            Assert.Equal(owner, projectInfo.Owner);
            Assert.Equal(repository, projectInfo.Repository);
        }


        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("\t")]
        [InlineData("  ")]
        [InlineData("not-a-url")]
        [InlineData("http://github.com/owner/another-name/repo.git")] // to many segments in the path
        [InlineData("ftp://github.com/owner/repo.git")] // unsupported scheme
        public void TryParseRemoteUrl_returns_false_for_invalid_input(string? url)
        {
            Assert.False(GitHubUrlParser.TryParseRemoteUrl(url!, out var uri));
        }

        [Theory]
        [InlineData("http://github.com/owner/repo-name.git", "github.com", "owner", "repo-name")]
        [InlineData("https://github.com/owner/repo-name.git", "github.com", "owner", "repo-name")]
        [InlineData("git@github.com:owner/repo-name.git", "github.com", "owner", "repo-name")]
        public void TryParseRemoteUrl_returns_the_expected_GitHubProjectInfo(string url, string host, string owner, string repository)
        {
            // ARRANGE

            // ACT 
            var success = GitHubUrlParser.TryParseRemoteUrl(url, out var projectInfo);

            // ASSERT
            Assert.True(success);
            Assert.NotNull(projectInfo);
            Assert.Equal(host, projectInfo!.Host);
            Assert.Equal(owner, projectInfo.Owner);
            Assert.Equal(repository, projectInfo.Repository);
        }
    }
}
