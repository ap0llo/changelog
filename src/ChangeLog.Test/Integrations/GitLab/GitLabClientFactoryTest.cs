using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.Integrations.GitLab;
using Xunit;

namespace Grynwald.ChangeLog.Test.Integrations.GitLab
{
    public class GitLabClientFactoryTest
    {
        [Theory]
        [InlineData("gitlab.com", "https://gitlab.com/api/v4/")]
        [InlineData("example.com", "https://example.com/api/v4/")]
        public void GetClient_returns_the_expected_client(string hostName, string expectedHostUrl)
        {
            // ARRANGE
            var sut = new GitLabClientFactory(new ChangeLogConfiguration());

            // ACT
            var client = sut.CreateClient(hostName);

            // ASSERT
            Assert.Equal(expectedHostUrl, client.HostUrl);
        }

        [Fact]
        public void CreateClient_succeeds_if_an_access_token_is_configured()
        {
            // ARRANGE
            var configuration = new ChangeLogConfiguration();
            configuration.Integrations.GitLab.AccessToken = "00000000000000000000";

            var sut = new GitLabClientFactory(configuration);

            // ACT
            var client = sut.CreateClient("gitlab.com");

            // ASSERT
            Assert.NotNull(client);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void CreateClient_succeeds_if_no_access_token_is_configured(string? accessToken)
        {
            // ARRANGE
            var configuration = new ChangeLogConfiguration();
            configuration.Integrations.GitLab.AccessToken = accessToken;

            var sut = new GitLabClientFactory(configuration);

            // ACT
            var client = sut.CreateClient("gitlab.com");

            // ASSERT
            Assert.NotNull(client);
        }
    }
}
