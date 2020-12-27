using System;
using Octokit;

namespace Grynwald.ChangeLog.Test.Integrations.GitHub
{
    public class TestGitHubCommit : GitHubCommit
    {
        public Uri HtmlUri => new Uri(HtmlUrl);

        public TestGitHubCommit(string htmlUrl)
        {
            HtmlUrl = htmlUrl;
        }


        public static TestGitHubCommit FromCommitSha(string sha)
        {
            return new TestGitHubCommit($"https://example.com/{sha}");
        }
    }
}
