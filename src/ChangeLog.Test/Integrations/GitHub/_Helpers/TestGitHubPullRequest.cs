using System;
using Octokit;

namespace Grynwald.ChangeLog.Test.Integrations.GitHub
{
    public class TestGitHubPullRequest : PullRequest
    {
        public Uri HtmlUri => new Uri(HtmlUrl);

        public TestGitHubPullRequest(string htmlUrl)
        {
            HtmlUrl = htmlUrl;
        }

        public static TestGitHubPullRequest FromPullRequestNumber(int prNumber)
        {
            return new TestGitHubPullRequest($"https://example.com/pr/{prNumber}");
        }
    }
}
