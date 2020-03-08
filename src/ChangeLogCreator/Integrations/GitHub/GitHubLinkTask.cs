using System;
using System.Linq;
using System.Threading.Tasks;
using ChangeLogCreator.Git;
using ChangeLogCreator.Model;
using ChangeLogCreator.Tasks;
using Octokit;

namespace ChangeLogCreator.Integrations.GitHub
{
    internal class GitHubLinkTask : IChangeLogTask
    {
        private readonly IGitRepository m_Repository;
        private readonly IGitHubClient m_GitHubClient;
        private readonly GitHubProjectInfo? m_ProjectInfo;

        //TODO: create client with host from project info
        public GitHubLinkTask(IGitRepository repository, IGitHubClient gitHubClient)
        {
            m_Repository = repository ?? throw new ArgumentNullException(nameof(repository));
            m_GitHubClient = gitHubClient ?? throw new ArgumentNullException(nameof(gitHubClient));

            var remote = m_Repository.Remotes.FirstOrDefault(r => StringComparer.OrdinalIgnoreCase.Equals(r.Name, "origin"));
            if (remote != null && GitHubUrlParser.TryParseRemoteUrl(remote.Url, out var projectInfo))
            {
                m_ProjectInfo = projectInfo;
            }
        }


        public async Task RunAsync(ChangeLog changeLog)
        {
            if (m_ProjectInfo == null)
                return;

            foreach (var versionChangeLog in changeLog.ChangeLogs)
            {
                foreach (var entry in versionChangeLog.AllEntries)
                {
                    await ProcessEntryAsync(entry);
                }
            }
        }


        private async Task ProcessEntryAsync(ChangeLogEntry entry)
        {
            var webUri = await GetCommitWebUriAsync(entry.Commit);

            if (webUri != null)
            {
                entry.CommitWebUri = webUri;
            }
        }

        private async Task<Uri?> GetCommitWebUriAsync(GitId commitId)
        {
            if (m_ProjectInfo == null)
                throw new InvalidOperationException();

            try
            {
                var commit = await m_GitHubClient.Repository.Commit.Get(m_ProjectInfo.Owner, m_ProjectInfo.Repository, commitId.Id);
                return new Uri(commit.HtmlUrl);
            }
            catch (Exception ex) when (ex is ApiValidationException || ex is NotFoundException)
            {
                return null;
            }
        }
    }
}
