using Octokit;

namespace Grynwald.ChangeLog.Integrations.GitHub
{
    internal interface IGitHubClientFactory
    {
        IGitHubClient CreateClient(string hostName);
    }
}

