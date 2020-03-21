using System;
using GitLabApiClient;
using Grynwald.ChangeLog.Configuration;

namespace Grynwald.ChangeLog.Integrations.GitLab
{
    internal class GitLabClientFactory : IGitLabClientFactory
    {
        private readonly ChangeLogConfiguration m_Configuration;

        public GitLabClientFactory(ChangeLogConfiguration configuration)
        {
            m_Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }


        public IGitLabClient CreateClient(string hostName)
        {
            var accessToken = m_Configuration.Integrations.GitLab.AccessToken ?? "";
            var client = new GitLabClient($"https://{hostName}/", accessToken);
            return client;
        }
    }
}
