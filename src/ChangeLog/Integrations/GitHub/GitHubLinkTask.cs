using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.Git;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Tasks;
using Microsoft.Extensions.Logging;
using Octokit;

namespace Grynwald.ChangeLog.Integrations.GitHub
{
    internal class GitHubLinkTask : IChangeLogTask
    {
        private const RegexOptions s_RegexOptions = RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled;
        private static readonly IReadOnlyList<Regex> s_GitHubReferencePatterns = new[]
        {
            new Regex("^((?<owner>[A-Z0-9-_.]+)/(?<repo>[A-Z0-9-_.]+))?#(?<id>\\d+)$", s_RegexOptions),
            new Regex("^GH-(?<id>\\d+)$", s_RegexOptions),
        };

        private readonly ILogger<GitHubLinkTask> m_Logger;
        private readonly ChangeLogConfiguration m_Configuration;
        private readonly IGitRepository m_Repository;
        private readonly IGitHubClientFactory m_GitHubClientFactory;


        public GitHubLinkTask(ILogger<GitHubLinkTask> logger, ChangeLogConfiguration configuration, IGitRepository repository, IGitHubClientFactory gitHubClientFactory)
        {
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            m_Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            m_Repository = repository ?? throw new ArgumentNullException(nameof(repository));
            m_GitHubClientFactory = gitHubClientFactory ?? throw new ArgumentNullException(nameof(gitHubClientFactory));
        }


        public async Task<ChangeLogTaskResult> RunAsync(ApplicationChangeLog changeLog)
        {
            var projectInfo = GetProjectInfo();
            if (projectInfo != null)
            {
                m_Logger.LogInformation($"Enabling GitHub integration with settings: " +
                    $"{nameof(projectInfo.Host)} = '{projectInfo.Host}', " +
                    $"{nameof(projectInfo.Owner)} = '{projectInfo.Owner}', " +
                    $"{nameof(projectInfo.Repository)} = '{projectInfo.Repository}'");
            }
            else
            {
                m_Logger.LogWarning("Failed to determine GitHub project name. Disabling GitHub integration");
                return ChangeLogTaskResult.Skipped;
            }

            m_Logger.LogInformation("Adding GitHub links to changelog");

            var githubClient = m_GitHubClientFactory.CreateClient(projectInfo.Host);

            var rateLimit = await githubClient.Miscellaneous.GetRateLimits();
            m_Logger.LogDebug($"GitHub rate limit: {rateLimit.Rate.Remaining} requests of {rateLimit.Rate.Limit} remaining");

            foreach (var versionChangeLog in changeLog.ChangeLogs)
            {
                foreach (var entry in versionChangeLog.AllEntries)
                {
                    await ProcessEntryAsync(projectInfo, githubClient, entry);
                }
            }

            return ChangeLogTaskResult.Success;
        }

        private GitHubProjectInfo? GetProjectInfo()
        {
            var host = m_Configuration.Integrations.GitHub.Host;
            var owner = m_Configuration.Integrations.GitHub.Owner;
            var repo = m_Configuration.Integrations.GitHub.Repository;

            // if all required properties were specified in the configuration, return project info
            if (!String.IsNullOrWhiteSpace(host) && !String.IsNullOrWhiteSpace(owner) && !String.IsNullOrWhiteSpace(repo))
            {
                m_Logger.LogDebug("Using GitHub project information from configuration");
                return new GitHubProjectInfo(host, owner, repo);
            }
            // otherwise try to determine the missing properties from the repository's remote url
            else
            {
                // get configured remote
                var remoteName = m_Configuration.Integrations.GitHub.RemoteName;
                m_Logger.LogDebug(
                    $"GitHub project information from configuration is incomplete. " +
                    $"Trying to get missing properties from git remote '{remoteName}'");

                var remote = m_Repository.Remotes.FirstOrDefault(r =>
                    StringComparer.OrdinalIgnoreCase.Equals(r.Name, remoteName)
                );

                if (remote == null)
                {
                    m_Logger.LogWarning($"Remote '{remoteName}' does not exist in the git repository.");
                    return null;
                }

                // if remote url could be parsed, replace missing properties with value from remote url
                if (GitHubUrlParser.TryParseRemoteUrl(remote.Url, out var parsedProjectInfo))
                {
                    if (String.IsNullOrWhiteSpace(host))
                    {
                        m_Logger.LogDebug($"Using GitHub host '{parsedProjectInfo.Host}' from remote url.");
                        host = parsedProjectInfo.Host;
                    }

                    if (String.IsNullOrWhiteSpace(owner))
                    {
                        m_Logger.LogDebug($"Using GitHub owner '{parsedProjectInfo.Owner}' from remote url.");
                        owner = parsedProjectInfo.Owner;
                    }

                    if (String.IsNullOrWhiteSpace(repo))
                    {
                        m_Logger.LogDebug($"Using GitHub repository '{parsedProjectInfo.Repository}' from remote url.");
                        repo = parsedProjectInfo.Repository;
                    }

                    return new GitHubProjectInfo(host, owner, repo);
                }
                else
                {
                    m_Logger.LogDebug($"Failed to determine GitHub project information from remote url '{remote.Url}'");
                }
            }

            return null;
        }

        private async Task ProcessEntryAsync(GitHubProjectInfo projectInfo, IGitHubClient githubClient, ChangeLogEntry entry)
        {
            m_Logger.LogDebug($"Adding links to entry {entry.Commit}");

            var webUri = await TryGetWebUriAsync(projectInfo, githubClient, entry.Commit);

            if (webUri != null)
            {
                entry.CommitWebUri = webUri;
            }
            else
            {
                m_Logger.LogWarning($"Failed to determine web uri for commit '{entry.Commit}'");
            }

            foreach (var footer in entry.Footers)
            {
                if (TryParseReference(projectInfo, footer.Value, out var owner, out var repo, out var id))
                {
                    var uri = await TryGetWebUriAsync(githubClient, owner, repo, id);
                    if (uri != null)
                    {
                        footer.WebUri = uri;
                    }
                    else
                    {
                        m_Logger.LogWarning($"Failed to determine web uri for reference '{owner}/{repo}#{id}'");
                    }
                }
            }
        }

        private async Task<Uri?> TryGetWebUriAsync(GitHubProjectInfo projectInfo, IGitHubClient githubClient, GitId commitId)
        {
            m_Logger.LogDebug($"Getting web uri for commit '{commitId}'");

            try
            {
                var commit = await githubClient.Repository.Commit.Get(projectInfo.Owner, projectInfo.Repository, commitId.Id);
                return new Uri(commit.HtmlUrl);
            }
            catch (Exception ex) when (ex is ApiValidationException || ex is NotFoundException)
            {
                return null;
            }
        }

        private bool TryParseReference(GitHubProjectInfo projectInfo, string input, [NotNullWhen(true)] out string? owner, [NotNullWhen(true)] out string? repo, out int id)
        {
            input = input.Trim();

            // using every pattern, try to get a issue/PR id from the input text
            foreach (var pattern in s_GitHubReferencePatterns)
            {
                var match = pattern.Match(input);

                if (match.Success)
                {
                    var idString = match.Groups["id"].ToString();
                    if (Int32.TryParse(idString, out id))
                    {
                        owner = match.Groups["owner"].Value;
                        repo = match.Groups["repo"].Value;

                        if (String.IsNullOrEmpty(owner))
                            owner = projectInfo.Owner;

                        if (String.IsNullOrEmpty(repo))
                            repo = projectInfo.Repository;

                        return true;
                    }
                }
            }

            id = -1;
            owner = null;
            repo = null;
            return false;
        }

        private async Task<Uri?> TryGetWebUriAsync(IGitHubClient githubClient, string owner, string repo, int id)
        {
            m_Logger.LogDebug($"Getting web uri for reference '{owner}/{repo}#{id}'");

            // check if the id is an issue
            var uri = await TryGetIssueWebUriAsync(githubClient, owner, repo, id);

            // if it is not an issue, check if it is a Pull Request Id
            if (uri == null)
            {
                uri = await TryGetPullRequestWebUriAsync(githubClient, owner, repo, id);
            }

            return uri;
        }

        private async Task<Uri?> TryGetIssueWebUriAsync(IGitHubClient githubClient, string owner, string repo, int id)
        {
            try
            {
                var issue = await githubClient.Issue.Get(owner, repo, id);
                return new Uri(issue.HtmlUrl);
            }
            catch (NotFoundException)
            {
                return null;
            }
        }

        private async Task<Uri?> TryGetPullRequestWebUriAsync(IGitHubClient githubClient, string owner, string repo, int id)
        {
            try
            {
                var pr = await githubClient.PullRequest.Get(owner, repo, id);
                return new Uri(pr.HtmlUrl);
            }
            catch (NotFoundException)
            {
                return null;
            }
        }

    }
}
