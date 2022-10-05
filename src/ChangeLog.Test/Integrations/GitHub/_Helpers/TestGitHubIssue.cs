using System;
using System.Linq;
using System.Reflection;
using Octokit;

namespace Grynwald.ChangeLog.Test.Integrations.GitHub
{
    public class TestGitHubIssue : Issue
    {
        public Uri HtmlUri => new Uri(HtmlUrl);

        public TestGitHubIssue(string htmlUrl)
        {
            // No really a good idea but probably okay for test code
            typeof(Issue)
                .GetProperty(nameof(HtmlUrl))!
                .SetMethod!
                .Invoke(this, new[] { htmlUrl });
        }

        public static TestGitHubIssue FromIssueNumber(int issueNumber)
        {
            return new TestGitHubIssue($"https://example.com/issue/{issueNumber}");
        }
    }
}
