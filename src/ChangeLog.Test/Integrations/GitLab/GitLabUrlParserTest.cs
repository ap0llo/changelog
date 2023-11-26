using System;
using Grynwald.ChangeLog.Integrations.GitLab;
using Xunit;

namespace Grynwald.ChangeLog.Test.Integrations.GitLab
{
    /// <summary>
    /// Unit tests for <see cref="GitLabUrlParser"/>
    /// </summary>
    public class GitLabUrlParserTest
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("\t")]
        [InlineData("  ")]
        [InlineData("not-a-url")]
        [InlineData("ftp://gitlab.com/owner/repo.git")] // unsupported scheme
        [InlineData("http://gitlab.com")]               // missing project path
        [InlineData("http://gitlab.com/user")]          // missing project name
        public void ParseRemoteUrl_throws_ArgumentException_for_invalid_input(string? url)
        {
            Assert.ThrowsAny<ArgumentException>(() => GitLabUrlParser.ParseRemoteUrl(url!));
        }

        [Theory]
        [InlineData("https://gitlab.com/user/repoName.git", "gitlab.com", "user", "reponame")]
        [InlineData("https://gitlab.com/group/subgroup/repoName.git", "gitlab.com", "group/subgroup", "reponame")]
        [InlineData("https://example.com/user/repoName.git", "example.com", "user", "reponame")]
        [InlineData("git@gitlab.com:user/repoName.git", "gitlab.com", "user", "repoName")]
        [InlineData("git@gitlab.com:group/subgroup/repoName.git", "gitlab.com", "group/subgroup", "repoName")]
        [InlineData("git@example.com:user/repoName.git", "example.com", "user", "repoName")]
        [InlineData("git@example.com:group/subgroup/repoName.git", "example.com", "group/subgroup", "repoName")]
        public void ParseRemoteUrl_returns_the_expected_GitLabProjectInfo(string remoteUrl, string host, string @namespace, string proejctName)
        {
            // ARRANGE
            var expected = new GitLabProjectInfo(host, @namespace, proejctName);

            // ACT
            var actual = GitLabUrlParser.ParseRemoteUrl(remoteUrl);

            // ASSERT
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("\t")]
        [InlineData("  ")]
        [InlineData("not-a-url")]
        [InlineData("ftp://gitlab.com/owner/repo.git")] // unsupported scheme
        [InlineData("http://gitlab.com")]               // missing project path
        [InlineData("http://gitlab.com/user")]          // missing project name
        public void TryParseRemoteUrl_returns_false_for_invalid_input(string? url)
        {
            Assert.False(GitLabUrlParser.TryParseRemoteUrl(url!, out var uri));
        }

        [Theory]
        [InlineData("https://gitlab.com/user/repoName.git", "gitlab.com", "user", "reponame")]
        [InlineData("https://gitlab.com/group/subgroup/repoName.git", "gitlab.com", "group/subgroup", "reponame")]
        [InlineData("https://example.com/user/repoName.git", "example.com", "user", "reponame")]
        [InlineData("git@gitlab.com:user/repoName.git", "gitlab.com", "user", "repoName")]
        [InlineData("git@gitlab.com:group/subgroup/repoName.git", "gitlab.com", "group/subgroup", "repoName")]
        [InlineData("git@example.com:user/repoName.git", "example.com", "user", "repoName")]
        [InlineData("git@example.com:group/subgroup/repoName.git", "example.com", "group/subgroup", "repoName")]
        public void TryParseRemoteUrl_returns_the_expected_GitHubProjectInfo(string url, string host, string @namespace, string projectName)
        {
            // ARRANGE
            var expected = new GitLabProjectInfo(host, @namespace, projectName);
            // ACT 
            var success = GitLabUrlParser.TryParseRemoteUrl(url, out var projectInfo);

            // ASSERT
            Assert.True(success);
            Assert.Equal(expected, projectInfo);
        }
    }
}
