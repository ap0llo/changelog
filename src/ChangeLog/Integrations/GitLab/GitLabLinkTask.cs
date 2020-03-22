using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GitLabApiClient;
using Grynwald.ChangeLog.Git;
using Grynwald.ChangeLog.Model;
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
        private readonly IGitRepository m_Repository;
        private readonly IGitLabClientFactory m_ClientFactory;
        private readonly GitLabProjectInfo? m_ProjectInfo;


        public GitLabLinkTask(ILogger<GitLabLinkTask> logger, IGitRepository repository, IGitLabClientFactory clientFactory)
        {
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            m_Repository = repository ?? throw new ArgumentNullException(nameof(repository));
            m_ClientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));

            // TODO: Allow configuration of remote name
            // TODO: Allow bypassing parsing by setting project info in the config file
            var remote = m_Repository.Remotes.FirstOrDefault(r => StringComparer.OrdinalIgnoreCase.Equals(r.Name, "origin"));
            if (remote != null && GitLabUrlParser.TryParseRemoteUrl(remote.Url, out var projectInfo))
            {
                m_ProjectInfo = projectInfo;
            }
            else
            {
                m_Logger.LogWarning("Failed to determine GitLab project path. Disabling GitLab integration");
            }
        }


        /// <inheritdoc />
        public async Task RunAsync(ApplicationChangeLog changeLog)
        {
            if (m_ProjectInfo == null)
                return;

            m_Logger.LogInformation("Adding GitHub links to changelog");

            var gitlabClient = m_ClientFactory.CreateClient(m_ProjectInfo.Host);

            foreach (var versionChangeLog in changeLog.ChangeLogs)
            {
                foreach (var entry in versionChangeLog.AllEntries)
                {
                    await ProcessEntryAsync(gitlabClient, entry);
                }
            }
        }


        private async Task ProcessEntryAsync(IGitLabClient githubClient, ChangeLogEntry entry)
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
                if (TryParseReference(footer.Value, out var type, out var projectPath, out var id))
                {
                    var uri = await TryGetWebUriAsync(githubClient, type.Value, projectPath, id);
                    if (uri != null)
                    {
                        footer.WebUri = uri;
                    }
                    else
                    {
                        m_Logger.LogWarning($"Failed to determine web uri for GitLab {type} reference '{footer.Value}'");
                    }
                }
            }
        }

        private async Task<Uri?> TryGetWebUriAsync(IGitLabClient gitlabClient, GitId commitId)
        {
            if (m_ProjectInfo == null)
                throw new InvalidOperationException();

            m_Logger.LogDebug($"Getting web uri for commit '{commitId}'");

            try
            {
                var commit = await gitlabClient.Commits.GetAsync(m_ProjectInfo.ProjectPath, commitId.Id);
                return new Uri(commit.WebUrl);
            }
            catch (Exception ex) when (ex is GitLabException gitlabException && gitlabException.HttpStatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        private bool TryParseReference(string input, [NotNullWhen(true)] out GitLabReferenceType? type, [NotNullWhen(true)] out string? projectPath, out int id)
        {
            if (m_ProjectInfo == null)
                throw new InvalidOperationException();

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
                        projectPath = m_ProjectInfo.ProjectPath;
                        return true;
                    }
                    // project name without namespace => reference to another project within the same namespace
                    else if (String.IsNullOrEmpty(projectNamespace) && !String.IsNullOrEmpty(projectName))
                    {
                        projectPath = $"{m_ProjectInfo.Namespace}/{projectName}";
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
                var mergeRequests = await gitlabClient.MergeRequests.GetAsync(projectPath, query => { query.MergeRequestsIds.Add(id); });
                if (mergeRequests.Count == 1)
                {
                    return new Uri(mergeRequests.Single().WebUrl); ;
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
                var milestones = await gitlabClient.Projects.GetMilestonesAsync(projectPath, options =>
                {
                    options.MilestoneIds = new[] { id };
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
