using System.Collections.Generic;
using Grynwald.ChangeLog.Integrations.GitHub;

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
    }
}
