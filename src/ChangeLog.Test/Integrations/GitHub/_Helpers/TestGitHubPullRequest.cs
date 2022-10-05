using System;
using System.Linq;
using System.Reflection;
using Octokit;

namespace Grynwald.ChangeLog.Test.Integrations.GitHub
{
    public class TestGitHubPullRequest : PullRequest
    {
        public Uri HtmlUri => new Uri(HtmlUrl);

        public TestGitHubPullRequest(string htmlUrl)
        {
            // No really a good idea but probably okay for test code
            typeof(PullRequest)
                .GetProperty(nameof(HtmlUrl))!
                .SetMethod!
                .Invoke(this, new[] { htmlUrl });
        }

        public static TestGitHubPullRequest FromPullRequestNumber(int prNumber)
        {
            return new TestGitHubPullRequest($"https://example.com/pr/{prNumber}");
        }
    }
}
