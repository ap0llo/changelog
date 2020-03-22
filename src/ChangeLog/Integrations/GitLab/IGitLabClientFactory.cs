using GitLabApiClient;

namespace Grynwald.ChangeLog.Integrations.GitLab
{
    internal interface IGitLabClientFactory
    {
        IGitLabClient CreateClient(string hostName);
    }
}
