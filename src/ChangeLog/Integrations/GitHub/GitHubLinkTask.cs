using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.Git;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Model.Text;
using Grynwald.ChangeLog.Pipeline;
using Grynwald.ChangeLog.Tasks;
using Microsoft.Extensions.Logging;
using Octokit;

namespace Grynwald.ChangeLog.Integrations.GitHub
{
    /// <summary>
    /// Detects and inserts links to GitHub for pull requests, issues and commits
    /// </summary>
    [BeforeTask(typeof(RenderTemplateTask))]
    [AfterTask(typeof(ParseCommitsTask))]
    // AddCommitFooterTask must run before GitHubLinkTask so a web link can be added to the "Commit" footer
    [AfterTask(typeof(AddCommitFooterTask))]
    internal sealed class GitHubLinkTask : IChangeLogTask
    {
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
            if (projectInfo is not null)
            {
                m_Logger.LogDebug($"Enabling GitHub integration with settings: " +
                    $"{nameof(projectInfo.Host)} = '{projectInfo.Host}', " +
                    $"{nameof(projectInfo.Owner)} = '{projectInfo.Owner}', " +
                    $"{nameof(projectInfo.Repository)} = '{projectInfo.Repository}'");
            }
            else
            {
                m_Logger.LogWarning("Failed to determine GitHub project name. Disabling GitHub integration");
                return ChangeLogTaskResult.Skipped;
            }

            m_Logger.LogInformation("Adding GitHub links to change log");

            var githubClient = m_GitHubClientFactory.CreateClient(projectInfo.Host);

            var rateLimit = await githubClient.RateLimit.GetRateLimits();
            m_Logger.LogDebug($"GitHub rate limit: {rateLimit.Rate.Remaining} requests of {rateLimit.Rate.Limit} remaining");


            try
            {
                foreach (var versionChangeLog in changeLog.ChangeLogs)
                {
                    foreach (var entry in versionChangeLog.AllEntries)
                    {
                        await ProcessEntryAsync(projectInfo, githubClient, entry);
                    }
                }

                return ChangeLogTaskResult.Success;
            }
            catch (RateLimitExceededException rateLimitExceededException)
            {
                var messageBuilder = new StringBuilder();
                messageBuilder.Append($"GitHub API rate limit exceeded (limit {rateLimitExceededException.Limit}). ");
                if (githubClient.Connection.Credentials.AuthenticationType == AuthenticationType.Anonymous)
                {
                    messageBuilder.Append("Consider using an Access Token for GitHub. Authenticated requests are given a higher rate limit.");
                }
                m_Logger.LogError(rateLimitExceededException, messageBuilder.ToString());
                return ChangeLogTaskResult.Error;
            }
            catch (ApiException ex)
            {
                m_Logger.LogError(ex, ex.Message);
                return ChangeLogTaskResult.Error;
            }
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

                if (remote is null)
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

            foreach (var footer in entry.Footers)
            {
                if (footer.Value is CommitReferenceTextElement commitReference)
                {
                    var uri = await TryGetWebUriAsync(githubClient, projectInfo, commitReference.CommitId);
                    if (uri is not null)
                    {
                        footer.Value = CommitReferenceTextElementWithWebLink.FromCommitReference(commitReference, uri);
                    }
                    else
                    {
                        m_Logger.LogWarning($"Failed to determine web uri for commit '{commitReference.CommitId}'");
                    }
                }
                else if (footer.Value is PlainTextElement && GitHubReference.TryParse(footer.Value.Text, projectInfo, out var reference))
                {
                    var uri = await TryGetWebUriAsync(githubClient, reference);
                    if (uri is not null)
                    {
                        footer.Value = new GitHubReferenceTextElement(footer.Value.Text, uri, projectInfo, reference);
                    }
                    else
                    {
                        m_Logger.LogWarning($"Failed to determine web uri for reference '{reference}'");
                    }
                }
                // Detect references to files from the reppository.
                // Inspired by GiLab's "repository file references", see https://docs.gitlab.com/ee/user/markdown.html#gitlab-specific-references
                else if (footer.Value is PlainTextElement && Uri.TryCreate(footer.Value.Text, UriKind.Relative, out var relativeUri))
                {
                    var relativePath = relativeUri.ToString();
                    var uri = await TryGetRepositoryFileLink(githubClient, projectInfo, relativePath);
                    if (uri is not null)
                    {
                        footer.Value = new GitHubFileReferenceTextElement(footer.Value.Text, uri, relativeUri.ToString());
                    }
                }
                // TODO: Also detect file references in Markdown links (like [Description](./docs/some-docs.md)
            }
        }

        private async Task<Uri?> TryGetWebUriAsync(IGitHubClient githubClient, GitHubProjectInfo projectInfo, GitId commitId)
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

        private async Task<Uri?> TryGetWebUriAsync(IGitHubClient githubClient, GitHubReference reference)
        {
            m_Logger.LogDebug($"Getting web uri for reference '{reference}'");

            // check if the id is an issue
            var uri = await TryGetIssueWebUriAsync(githubClient, reference);

            // if it is not an issue, check if it is a Pull Request Id
            if (uri is null)
            {
                uri = await TryGetPullRequestWebUriAsync(githubClient, reference);
            }

            return uri;
        }

        private async Task<Uri?> TryGetIssueWebUriAsync(IGitHubClient githubClient, GitHubReference reference)
        {
            try
            {
                var issue = await githubClient.Issue.Get(reference.Project.Owner, reference.Project.Repository, reference.Id);
                return new Uri(issue.HtmlUrl);
            }
            catch (NotFoundException)
            {
                return null;
            }
        }

        private async Task<Uri?> TryGetPullRequestWebUriAsync(IGitHubClient githubClient, GitHubReference reference)
        {
            try
            {
                var pr = await githubClient.PullRequest.Get(reference.Project.Owner, reference.Project.Repository, reference.Id);
                return new Uri(pr.HtmlUrl);
            }
            catch (NotFoundException)
            {
                return null;
            }
        }

        private async Task<Uri?> TryGetRepositoryFileLink(IGitHubClient gitHubClient, GitHubProjectInfo project, string relativePath)
        {
            //TODO: Recognize links to file numbers
            //TODO: Normalize file paths?
            try
            {
                var files = await gitHubClient.Repository.Content.GetAllContents(project.Owner, project.Repository, relativePath);

                // relativePath can be a path to either a file or a directory in the GitHub repository
                // - If the path is a file, GetAllContents() will return a single item with a non-null Content
                // - If there are multiple items in the response, relativePath is a directory path => ignore
                // - If the response is a single item and Content is null, relativePath is path to a directory that contains a single file => ignore
                if (files is [{ Content: not null } file])
                {
                    return new Uri(file.HtmlUrl);
                }
            }
            // Not found => ignore link
            catch (NotFoundException)
            {
                return null;
            }

            return null;
        }
    }
}
