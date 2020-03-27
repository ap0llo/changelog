using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
        private readonly IGitRepository m_Repository;
        private readonly IGitHubClientFactory m_GitHubClientFactory;
        private readonly GitHubProjectInfo? m_ProjectInfo;


        public GitHubLinkTask(ILogger<GitHubLinkTask> logger, IGitRepository repository, IGitHubClientFactory gitHubClientFactory)
        {
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            m_Repository = repository ?? throw new ArgumentNullException(nameof(repository));
            m_GitHubClientFactory = gitHubClientFactory ?? throw new ArgumentNullException(nameof(gitHubClientFactory));

            // TODO: Allow configuration of remote name
            // TODO: Allow bypassing parsing by setting project info in the config file
            var remote = m_Repository.Remotes.FirstOrDefault(r => StringComparer.OrdinalIgnoreCase.Equals(r.Name, "origin"));
            if (remote != null && GitHubUrlParser.TryParseRemoteUrl(remote.Url, out var projectInfo))
            {
                m_ProjectInfo = projectInfo;
            }
            else
            {
                m_Logger.LogWarning("Failed to determine GitHub project name. Disabling GitHub integration");
            }
        }


        public async Task RunAsync(ApplicationChangeLog changeLog)
        {
            if (m_ProjectInfo == null)
                return;

            m_Logger.LogInformation("Adding GitHub links to changelog");

            var githubClient = m_GitHubClientFactory.CreateClient(m_ProjectInfo.Host);

            var rateLimit = await githubClient.Miscellaneous.GetRateLimits();
            m_Logger.LogDebug($"GitHub rate limit: {rateLimit.Rate.Remaining} requests of {rateLimit.Rate.Limit} remaining");

            foreach (var versionChangeLog in changeLog.ChangeLogs)
            {
                foreach (var entry in versionChangeLog.AllEntries)
                {
                    await ProcessEntryAsync(githubClient, entry);
                }
            }
        }


        private async Task ProcessEntryAsync(IGitHubClient githubClient, ChangeLogEntry entry)
        {
            m_Logger.LogDebug($"Adding links to entry {entry.Commit}");

            var webUri = await TryGetWebUriAsync(githubClient, entry.Commit);

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
                if (TryParseReference(footer.Value, out var owner, out var repo, out var id))
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

        private async Task<Uri?> TryGetWebUriAsync(IGitHubClient githubClient, GitId commitId)
        {
            if (m_ProjectInfo == null)
                throw new InvalidOperationException();

            m_Logger.LogDebug($"Getting web uri for commit '{commitId}'");

            try
            {
                var commit = await githubClient.Repository.Commit.Get(m_ProjectInfo.Owner, m_ProjectInfo.Repository, commitId.Id);
                return new Uri(commit.HtmlUrl);
            }
            catch (Exception ex) when (ex is ApiValidationException || ex is NotFoundException)
            {
                return null;
            }
        }

        private bool TryParseReference(string input, [NotNullWhen(true)] out string? owner, [NotNullWhen(true)] out string? repo, out int id)
        {
            if (m_ProjectInfo == null)
                throw new InvalidOperationException();


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
                            owner = m_ProjectInfo.Owner;

                        if (String.IsNullOrEmpty(repo))
                            repo = m_ProjectInfo.Repository;

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
            if (m_ProjectInfo == null)
                throw new InvalidOperationException();

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
            if (m_ProjectInfo == null)
                throw new InvalidOperationException();

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
