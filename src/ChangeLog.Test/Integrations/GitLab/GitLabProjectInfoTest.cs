using Grynwald.ChangeLog.Integrations.GitLab;
using Xunit;

namespace Grynwald.ChangeLog.Test.Integrations.GitLab
{
    public class GitLabProjectInfoTest
    {
        [Theory]
        [InlineData("example.com", "example.com", "user/repo", "user/repo")]
        // Comparisons must be case-insensitive
        [InlineData("EXAMPLE.COM", "example.com", "user/repo", "user/repo")]
        [InlineData("example.com", "example.com", "USER/repo", "user/repo")]
        [InlineData("example.com", "example.com", "user/REPO", "user/repo")]
        [InlineData("example.com", "example.com", "user/repo", "USER/repo")]
        [InlineData("example.com", "example.com", "user/repo", "user/REPO")]
        public void Equals_returns_true_is_all_properties_are_equal(string host1, string host2, string projectPath1, string projectPath2)
        {
            var instance1 = new GitLabProjectInfo(host1, projectPath1);
            var instance2 = new GitLabProjectInfo(host2, projectPath2);

            Assert.Equal(instance1, instance2);
            Assert.Equal(instance1.GetHashCode(), instance2.GetHashCode());
        }

    }
}
