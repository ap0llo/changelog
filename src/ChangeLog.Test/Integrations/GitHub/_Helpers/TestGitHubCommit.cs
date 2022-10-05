using System;
using System.Linq;
using System.Reflection;
using Octokit;

namespace Grynwald.ChangeLog.Test.Integrations.GitHub
{
    public class TestGitHubCommit : GitHubCommit
    {
        public Uri HtmlUri => new Uri(HtmlUrl);

        public TestGitHubCommit(string htmlUrl)
        {
            // No really a good idea but probably okay for test code
            typeof(GitHubCommit)
                .GetProperty(nameof(HtmlUrl))!
                .SetMethod!
                .Invoke(this, new[] { htmlUrl });
        }


        public static TestGitHubCommit FromCommitSha(string sha)
        {
            return new TestGitHubCommit($"https://example.com/{sha}");
        }
    }
}
