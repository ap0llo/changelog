using System;
using Grynwald.ChangeLog.Configuration;
using Octokit;

namespace Grynwald.ChangeLog.Integrations.GitHub
{
    internal class GitHubClientFactory : IGitHubClientFactory
    {
        private readonly ChangeLogConfiguration m_Configuration;

        public GitHubClientFactory(ChangeLogConfiguration configuration)
        {
            m_Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }


        public IGitHubClient CreateClient(string hostName)
        {
            var client = new GitHubClient(new ProductHeaderValue("changelog-creator"), new Uri($"https://{hostName}/"));

            var accessToken = m_Configuration.Integrations.GitHub.AccessToken;
            if (!String.IsNullOrEmpty(accessToken))
            {
                client.Credentials = new Credentials(accessToken);
            }

            return client;
        }
    }
}
