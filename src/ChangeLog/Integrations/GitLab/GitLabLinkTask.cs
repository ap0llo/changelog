using System;
using System.Linq;
using System.Net;
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
    }
}
