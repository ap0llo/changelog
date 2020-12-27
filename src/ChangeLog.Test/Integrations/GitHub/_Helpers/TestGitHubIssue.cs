using System;
using Octokit;

namespace Grynwald.ChangeLog.Test.Integrations.GitHub
{
    public class TestGitHubIssue : Issue
    {
        public Uri HtmlUri => new Uri(HtmlUrl);

        public TestGitHubIssue(string htmlUrl)
        {
            HtmlUrl = htmlUrl;
        }

        public static TestGitHubIssue FromIssueNumber(int issueNumber)
        {
            return new TestGitHubIssue($"https://example.com/issue/{issueNumber}");
        }
    }
}
