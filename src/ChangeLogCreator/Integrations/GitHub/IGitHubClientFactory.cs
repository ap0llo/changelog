using Octokit;

namespace ChangeLogCreator.Integrations.GitHub
{
    internal interface IGitHubClientFactory
    {
        IGitHubClient CreateClient(string hostName);
    }
}

