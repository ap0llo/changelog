using System;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.Integrations.GitHub;
using Octokit;
using Xunit;

namespace Grynwald.ChangeLog.Test.Integrations.GitHub
{
    public class GitHubClientFactoryTest
    {
        [Theory]
        [InlineData("github.com", "https://api.github.com/")]
        [InlineData("api.github.com", "https://api.github.com/")]
        [InlineData("github.myenterprise.com", "https://github.myenterprise.com/api/v3/")]
        public void GetClient_returns_the_expected_client(string hostName, string expectedBaseAddress)
        {
            // ARRANGE
            var sut = new GitHubClientFactory(new ChangeLogConfiguration());

            // ACT
            var client = sut.CreateClient(hostName);

            // ASSERT
            Assert.Equal(new Uri(expectedBaseAddress), client.Connection.BaseAddress);
        }

        [Fact]
        public void CreateClient_adds_an_access_token_if_token_is_configured()
        {
            // ARRANGE
            var configuration = new ChangeLogConfiguration();
            configuration.Integrations.GitHub.AccessToken = "some-access-token";

            var sut = new GitHubClientFactory(configuration);

            // ACT
            var client = sut.CreateClient("github.com");

            // ASSERT
            Assert.NotNull(client);
            var clientConcrete = Assert.IsType<GitHubClient>(client);
            Assert.Equal(AuthenticationType.Oauth, clientConcrete.Credentials.AuthenticationType);
            Assert.Null(clientConcrete.Credentials.Login);
            Assert.NotNull(clientConcrete.Credentials.Password);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void CreateClient_succeeds_if_no_access_token_is_configured(string accessToken)
        {
            // ARRANGE
            var configuration = new ChangeLogConfiguration();
            configuration.Integrations.GitHub.AccessToken = accessToken;

            var sut = new GitHubClientFactory(configuration);

            // ACT
            var client = sut.CreateClient("github.com");

            // ASSERT
            Assert.NotNull(client);
            var clientConcrete = Assert.IsType<GitHubClient>(client);
            Assert.Equal(AuthenticationType.Anonymous, clientConcrete.Credentials.AuthenticationType);
            Assert.Null(clientConcrete.Credentials.Login);
            Assert.Null(clientConcrete.Credentials.Password);
        }

    }
}
