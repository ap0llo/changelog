using System;
using System.Linq;
using System.Threading.Tasks;
using ChangeLogCreator.Git;
using ChangeLogCreator.Model;
using ChangeLogCreator.Tasks;

namespace ChangeLogCreator.Integrations.GitHub
{
    internal class GitHubLinkTask : IChangeLogTask
    {
        private readonly IGitRepository m_Repository;
        private readonly GitHubProjectInfo? m_ProjectInfo;


        public GitHubLinkTask(IGitRepository repository)
        {
            m_Repository = repository ?? throw new ArgumentNullException(nameof(repository));

            var remote = m_Repository.Remotes.FirstOrDefault(r => StringComparer.OrdinalIgnoreCase.Equals(r.Name, "origin"));
            if (remote != null && GitHubUrlParser.TryParseRemoteUrl(remote.Url, out var projectInfo))
            {
                m_ProjectInfo = projectInfo;
            }
        }


        public Task RunAsync(ChangeLog changeLog)
        {
            if (m_ProjectInfo == null)
                return Task.CompletedTask;

            foreach (var versionChangeLog in changeLog.ChangeLogs)
            {
                foreach (var entry in versionChangeLog.AllEntries)
                {
                    entry.CommitWebUri = GetCommitWebUri(entry.Commit);
                }
            }

            return Task.CompletedTask;
        }


        private Uri GetCommitWebUri(GitId commit)
        {
            if (m_ProjectInfo == null)
                throw new InvalidOperationException();

            return new Uri($"https://{m_ProjectInfo.Host}/{m_ProjectInfo.Owner}/{m_ProjectInfo.Repository}/commit/{commit.Id}");
        }
    }
}
