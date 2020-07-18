using System.Collections.Generic;
using Grynwald.ChangeLog.Integrations.GitLab;

namespace Grynwald.ChangeLog.Test.Integrations.GitLab
{
    /// <summary>
    /// Tests for <see cref="GitLabProjectInfo"/>
    /// </summary>
    public class GitLabProjectInfoTest : EqualityTest<GitLabProjectInfo, GitLabProjectInfoTest>, IEqualityTestDataProvider<GitLabProjectInfo>
    {
        public IEnumerable<(GitLabProjectInfo left, GitLabProjectInfo right)> GetEqualTestCases()
        {
            yield return (
                new GitLabProjectInfo("example.com", "user", "repo"),
                new GitLabProjectInfo("example.com", "user", "repo")
            );
            yield return (
                new GitLabProjectInfo("example.com", "group/subgroup", "repo"),
                new GitLabProjectInfo("example.com", "group/subgroup", "repo")
            );

            // Comparisons must be case-insensitive
            yield return (
                new GitLabProjectInfo("example.com", "user", "repo"),
                new GitLabProjectInfo("EXAMPLE.COM", "user", "repo")
            );
            yield return (
                new GitLabProjectInfo("example.com", "group/subgroup", "repo"),
                new GitLabProjectInfo("EXAMPLE.COM", "group/subgroup", "repo")
            );

            yield return (
                new GitLabProjectInfo("example.com", "user", "repo"),
                new GitLabProjectInfo("example.com", "USER", "repo")
            );
            yield return (
                new GitLabProjectInfo("example.com", "group/subgroup", "repo"),
                new GitLabProjectInfo("example.com", "GROUP/SUBGROUP", "repo")
            );
            yield return (
                new GitLabProjectInfo("example.com", "user", "repo"),
                new GitLabProjectInfo("example.com", "user", "REPO")
            );
            yield return (
                new GitLabProjectInfo("example.com", "group/subgroup", "repo"),
                new GitLabProjectInfo("example.com", "group/subgroup", "REPO")
            );
            yield return (
                new GitLabProjectInfo("example.com", "group/subgroup", "repo"),
                new GitLabProjectInfo("example.com", "group/SUBGROUP", "repo")
            );
        }

        public IEnumerable<(GitLabProjectInfo left, GitLabProjectInfo right)> GetUnequalTestCases()
        {
            yield return (
                new GitLabProjectInfo("example.com", "user", "repo"),
                new GitLabProjectInfo("example.net", "user", "repo")
            );
            yield return (
                new GitLabProjectInfo("example.com", "group/subgroup", "repo"),
                new GitLabProjectInfo("example.net", "group/subgroup", "repo")
            );
            yield return (
                new GitLabProjectInfo("example.com", "user1", "repo"),
                new GitLabProjectInfo("example.com", "user2", "repo")
            );
            yield return (
                new GitLabProjectInfo("example.com", "group1/subgroup", "repo"),
                new GitLabProjectInfo("example.com", "group2/subgroup", "repo")
            );
            yield return (
                new GitLabProjectInfo("example.com", "user", "repo1"),
                new GitLabProjectInfo("example.com", "user", "repo2")
            );
            yield return (
                new GitLabProjectInfo("example.com", "group/subgroup", "repo1"),
                new GitLabProjectInfo("example.com", "group/subgroup", "repo2")
            );
        }
    }
}
