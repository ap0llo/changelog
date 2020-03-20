using Grynwald.ChangeLog.Integrations.GitHub;
using Xunit;

namespace Grynwald.ChangeLog.Test.Integrations.GitHub
{
    public class GitHubProjectInfoTest
    {
        [Theory]
        [InlineData("example.com", "example.com", "user", "user", "repo", "repo")]
        // Comparisons must be case-insensitive
        [InlineData("EXAMPLE.COM", "example.com", "user", "user", "repo", "repo")]
        [InlineData("example.com", "example.com", "USER", "user", "repo", "repo")]
        [InlineData("example.com", "example.com", "user", "user", "REPO", "repo")]
        [InlineData("example.com", "EXAMPLE.com", "user", "user", "repo", "repo")]
        [InlineData("example.com", "example.com", "user", "USER", "repo", "repo")]
        [InlineData("example.com", "example.com", "user", "user", "repo", "REPO")]
        public void Equals_returns_true_is_all_properties_are_equal(string host1, string host2, string owner1, string owner2, string repo1, string repo2)
        {
            var instance1 = new GitHubProjectInfo(host1, owner1, repo1);
            var instance2 = new GitHubProjectInfo(host2, owner2, repo2);

            Assert.Equal(instance1, instance2);
            Assert.Equal(instance1.GetHashCode(), instance2.GetHashCode());

        }
    }
}
