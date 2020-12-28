using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GitLabApiClient;
using GitLabApiClient.Models.MergeRequests.Requests;
using GitLabApiClient.Models.Milestones.Responses;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.Git;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Model.Text;
using Grynwald.ChangeLog.Tasks;
using Microsoft.Extensions.Logging;

namespace Grynwald.ChangeLog.Integrations.GitLab
{
    internal sealed class GitLabLinkTask : IChangeLogTask
    {
        private enum GitLabReferenceType
        {
            Issue,
            MergeRequest,
            Milestone
        }

        private const RegexOptions s_RegexOptions = RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled;
        private static readonly Regex s_GitLabReferencePattern = new Regex("^((?<namespace>[A-Z0-9-_./]+)/)?(?<project>[A-Z0-9-_.]+)?(?<type>(#|!|%))(?<id>\\d+)$", s_RegexOptions);


        private readonly ILogger<GitLabLinkTask> m_Logger;
        private readonly ChangeLogConfiguration m_Configuration;
        private readonly IGitRepository m_Repository;
        private readonly IGitLabClientFactory m_ClientFactory;


        public GitLabLinkTask(ILogger<GitLabLinkTask> logger, ChangeLogConfiguration configuration, IGitRepository repository, IGitLabClientFactory clientFactory)
        {
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            m_Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            m_Repository = repository ?? throw new ArgumentNullException(nameof(repository));
            m_ClientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
        }


        /// <inheritdoc />
        public async Task<ChangeLogTaskResult> RunAsync(ApplicationChangeLog changeLog)
        {
            var projectInfo = GetProjectInfo();
            if (projectInfo != null)
            {
                m_Logger.LogDebug($"Enabling GitLab integration with settings: " +
                    $"{nameof(projectInfo.Host)} = '{projectInfo.Host}', " +
                    $"{nameof(projectInfo.Namespace)} = '{projectInfo.Namespace}', " +
                    $"{nameof(projectInfo.Project)} = '{projectInfo.Project}'");
            }
            else
            {
                m_Logger.LogWarning("Failed to determine GitLab project information. Disabling GitLab integration");
                return ChangeLogTaskResult.Skipped;
            }

            m_Logger.LogInformation("Adding GitLab links to change log");

            var gitlabClient = m_ClientFactory.CreateClient(projectInfo.Host);

            foreach (var versionChangeLog in changeLog.ChangeLogs)
            {
                foreach (var entry in versionChangeLog.AllEntries)
                {
                    await ProcessEntryAsync(projectInfo, gitlabClient, entry);
                }
            }

            return ChangeLogTaskResult.Success;
        }


        private GitLabProjectInfo? GetProjectInfo()
        {
            var host = m_Configuration.Integrations.GitLab.Host;
            var @namespace = m_Configuration.Integrations.GitLab.Namespace;
            var project = m_Configuration.Integrations.GitLab.Project;

            // if all required properties were specified in the configuration, return project info
            if (!String.IsNullOrWhiteSpace(host) && !String.IsNullOrWhiteSpace(@namespace) && !String.IsNullOrWhiteSpace(project))
            {
                m_Logger.LogDebug("Using GitLab project information from configuration");
                return new GitLabProjectInfo(host, @namespace, project);
            }
            // otherwise, try to determine the missing properties from the repository's remote url
            else
            {
                // get configured remote
                var remoteName = m_Configuration.Integrations.GitLab.RemoteName;
                m_Logger.LogDebug(
                    $"GitLab project information from configuration is incomplete. " +
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
                if (GitLabUrlParser.TryParseRemoteUrl(remote.Url, out var parsedProjectInfo))
                {
                    if (String.IsNullOrWhiteSpace(host))
                    {
                        m_Logger.LogDebug($"Using GitLab host '{parsedProjectInfo.Host}' from remote url.");
                        host = parsedProjectInfo.Host;
                    }

                    if (String.IsNullOrWhiteSpace(@namespace))
                    {
                        m_Logger.LogDebug($"Using GitLab namespace '{parsedProjectInfo.Namespace}' from remote url.");
                        @namespace = parsedProjectInfo.Namespace;
                    }

                    if (String.IsNullOrWhiteSpace(project))
                    {
                        m_Logger.LogDebug($"Using GitLab project name '{parsedProjectInfo.Project}' from remote url.");
                        project = parsedProjectInfo.Project;
                    }

                    return new GitLabProjectInfo(host, @namespace, project);
                }
                else
                {
                    m_Logger.LogDebug($"Failed to determine GitLab project information from remote url '{remote.Url}'");
                }
            }

            return null;
        }

        private async Task ProcessEntryAsync(GitLabProjectInfo projectInfo, IGitLabClient gitlabClient, ChangeLogEntry entry)
        {
            m_Logger.LogDebug($"Adding links to entry {entry.Commit}");

            foreach (var footer in entry.Footers)
            {
                if (footer.Value is CommitReferenceTextElement commitReference)
                {
                    var uri = await TryGetWebUriAsync(projectInfo, gitlabClient, commitReference.CommitId);
                    if (uri is not null)
                    {
                        footer.Value = CommitReferenceTextElementWithWebLink.FromCommitReference(commitReference, uri);
                    }
                    else
                    {
                        m_Logger.LogWarning($"Failed to determine web uri for commit '{entry.Commit}'");
                    }
                }
                else if (footer.Value is PlainTextElement && TryParseReference(projectInfo, footer.Value.Text, out var type, out var projectPath, out var id))
                {
                    var uri = await TryGetWebUriAsync(gitlabClient, type.Value, projectPath, id);
                    if (uri is not null)
                    {
                        footer.Value = new WebLinkTextElement(footer.Value.Text, uri);
                    }
                    else
                    {
                        m_Logger.LogWarning($"Failed to determine web uri for GitLab {type} reference '{footer.Value}'");
                    }
                }
            }
        }

        private async Task<Uri?> TryGetWebUriAsync(GitLabProjectInfo projectInfo, IGitLabClient gitlabClient, GitId commitId)
        {
            m_Logger.LogDebug($"Getting web uri for commit '{commitId}'");

            try
            {
                var commit = await gitlabClient.Commits.GetAsync(projectInfo.ProjectPath, commitId.Id);
                return new Uri(commit.WebUrl);
            }
            catch (Exception ex) when (ex is GitLabException gitlabException && gitlabException.HttpStatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        private bool TryParseReference(GitLabProjectInfo projectInfo, string input, [NotNullWhen(true)] out GitLabReferenceType? type, [NotNullWhen(true)] out string? projectPath, out int id)
        {
            input = input.Trim();

            var match = s_GitLabReferencePattern.Match(input);

            if (match.Success)
            {
                var idString = match.Groups["id"].ToString();
                if (Int32.TryParse(idString, out id))
                {
                    var projectNamespace = match.Groups["namespace"].Value;
                    var projectName = match.Groups["project"].Value;
                    var typeString = match.Groups["type"].Value;

                    switch (typeString)
                    {
                        case "#":
                            type = GitLabReferenceType.Issue;
                            break;
                        case "!":
                            type = GitLabReferenceType.MergeRequest;
                            break;
                        case "%":
                            type = GitLabReferenceType.Milestone;
                            break;

                        default:
                            type = null;
                            projectPath = null;
                            id = -1;
                            return false;
                    }

                    // no project name or namespace => reference within the current project
                    if (String.IsNullOrEmpty(projectNamespace) && String.IsNullOrEmpty(projectName))
                    {
                        projectPath = projectInfo.ProjectPath;
                        return true;
                    }
                    // project name without namespace => reference to another project within the same namespace
                    else if (String.IsNullOrEmpty(projectNamespace) && !String.IsNullOrEmpty(projectName))
                    {
                        projectPath = $"{projectInfo.Namespace}/{projectName}";
                        return true;
                    }
                    // namespace and project name found => full reference
                    else if (!String.IsNullOrEmpty(projectNamespace) && !String.IsNullOrEmpty(projectName))
                    {
                        projectPath = $"{projectNamespace}/{projectName}";
                        return true;
                    }
                }
            }

            type = null;
            projectPath = null;
            id = -1;
            return false;
        }

        private Task<Uri?> TryGetWebUriAsync(IGitLabClient gitlabClient, GitLabReferenceType type, string projectPath, int id)
        {
            switch (type)
            {
                case GitLabReferenceType.Issue:
                    return TryGetIssueWebUriAsync(gitlabClient, projectPath, id);

                case GitLabReferenceType.MergeRequest:
                    return TryGetMergeRequestWebUriAsync(gitlabClient, projectPath, id);

                case GitLabReferenceType.Milestone:
                    return TryGetMilestoneWebUriAsync(gitlabClient, projectPath, id);

                default:
                    throw new InvalidOperationException();
            }
        }

        private async Task<Uri?> TryGetIssueWebUriAsync(IGitLabClient gitlabClient, string projectPath, int id)
        {
            m_Logger.LogDebug($"Getting web uri for issue {id}");

            try
            {
                var issue = await gitlabClient.Issues.GetAsync(projectPath, id);
                return new Uri(issue.WebUrl);
            }
            catch (Exception ex) when (ex is GitLabException gitlabException && gitlabException.HttpStatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        private async Task<Uri?> TryGetMergeRequestWebUriAsync(IGitLabClient gitlabClient, string projectPath, int id)
        {
            m_Logger.LogDebug($"Getting web uri for Merge Request {id}");

            try
            {
                var mergeRequests = await gitlabClient.MergeRequests.GetAsync(projectPath, queryOptions =>
                    {
                        queryOptions.MergeRequestsIds = new[] { id };
                        queryOptions.State = QueryMergeRequestState.All;
                    });

                if (mergeRequests.Count == 1)
                {
                    return new Uri(mergeRequests.Single().WebUrl);
                }
                else
                {
                    return null;
                }

            }
            catch (Exception ex) when (ex is GitLabException gitlabException && gitlabException.HttpStatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        private async Task<Uri?> TryGetMilestoneWebUriAsync(IGitLabClient gitlabClient, string projectPath, int id)
        {
            m_Logger.LogDebug($"Getting web uri for Milestone {id}");

            try
            {
                // use GetMilestonesAsync() instead of GetMilestoneAsync() because the id
                // we can pass to GetMilestoneAsync() is not the id of the milestone
                // within the project, but some other id.
                // Instead, use GetMilestonesAsync() and query for a single milestone id
                // for which we can use the id in the reference.
                var milestones = await gitlabClient.Projects.GetMilestonesAsync(projectPath, queryOptions =>
                {
                    queryOptions.MilestoneIds = new[] { id };
                    queryOptions.State = MilestoneState.All;
                });

                if (milestones.Count == 1)
                {
                    return new Uri(milestones.Single().WebUrl);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex) when (ex is GitLabException gitlabException && gitlabException.HttpStatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }
    }
}
