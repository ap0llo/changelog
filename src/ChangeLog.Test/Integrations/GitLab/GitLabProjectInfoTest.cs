using Grynwald.ChangeLog.Integrations.GitLab;
using Xunit;

namespace Grynwald.ChangeLog.Test.Integrations.GitLab
{
    public class GitLabProjectInfoTest
    {
        [Theory]
        [InlineData("example.com", "example.com", "user", "user", "repo", "repo")]
        [InlineData("example.com", "example.com", "group/subgroup", "group/subgroup", "repo", "repo")]
        // Comparisons must be case-insensitive
        [InlineData("EXAMPLE.COM", "example.com", "user", "user", "repo", "repo")]
        [InlineData("example.com", "example.com", "USER", "user", "repo", "repo")]
        [InlineData("example.com", "example.com", "user", "user", "REPO", "repo")]
        [InlineData("example.com", "example.com", "user", "USER", "repo", "repo")]
        [InlineData("example.com", "example.com", "user", "user", "repo", "REPO")]
        [InlineData("example.com", "example.com", "group/subgroup", "group/SUBGROUP", "repo", "repo")]
        [InlineData("example.com", "example.com", "GROUP/subgroup", "group/subgroup", "repo", "repo")]        
        public void Equals_returns_true_is_all_properties_are_equal(string host1, string host2, string namespace1, string namespace2, string project1, string project2)
        {
            var instance1 = new GitLabProjectInfo(host1, namespace1, project1);
            var instance2 = new GitLabProjectInfo(host2, namespace2, project2);

            Assert.Equal(instance1, instance2);
            Assert.Equal(instance1.GetHashCode(), instance2.GetHashCode());
        }

    }
}
