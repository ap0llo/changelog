using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ChangeLogCreator.Git;
using ChangeLogCreator.Model;
using ChangeLogCreator.Tasks;
using Octokit;

namespace ChangeLogCreator.Integrations.GitHub
{
    internal class GitHubLinkTask : IChangeLogTask
    {
        private const RegexOptions s_RegexOptions = RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled;
        private static readonly IReadOnlyList<Regex> s_GitHubReferencePatterns = new[]
        {
            new Regex("^((?<owner>[A-Z0-9-_.]+)/(?<repo>[A-Z0-9-_.]+))?#(?<id>\\d+)$", s_RegexOptions),
            new Regex("^GH-(?<id>\\d+)$", s_RegexOptions),
        };

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
            var webUri = await TryGetWebUriAsync(entry.Commit);

            if (webUri != null)
            {
                entry.CommitWebUri = webUri;
            }

            foreach (var footer in entry.Footers)
            {
                if (TryGetReferenceId(footer.Value, out var owner, out var repo, out var id))
                {
                    var uri = await TryGetWebUriAsync(owner, repo, id);
                    if (uri != null)
                    {
                        footer.WebUri = uri;
                    }
                }
            }
        }


        private async Task<Uri?> TryGetWebUriAsync(GitId commitId)
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

        private bool TryGetReferenceId(string input, [NotNullWhen(true)] out string? owner, [NotNullWhen(true)] out string? repo, out int id)
        {
            if (m_ProjectInfo == null)
                throw new InvalidOperationException();


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


        private async Task<Uri?> TryGetWebUriAsync(string owner, string repo, int id)
        {
            // check if the id is an issue
            var uri = await TryGetIssueWebUriAsync(owner, repo, id);

            // if it is not an issue, check if it is a Pull Request Id
            if (uri == null)
            {
                uri = await TryGetPullRequestWebUriAsync(owner, repo, id);
            }

            return uri;
        }

        private async Task<Uri?> TryGetIssueWebUriAsync(string owner, string repo, int id)
        {
            if (m_ProjectInfo == null)
                throw new InvalidOperationException();

            try
            {
                var issue = await m_GitHubClient.Issue.Get(owner, repo, id);
                return new Uri(issue.HtmlUrl);
            }
            catch (NotFoundException)
            {
                return null;
            }
        }

        private async Task<Uri?> TryGetPullRequestWebUriAsync(string owner, string repo, int id)
        {
            if (m_ProjectInfo == null)
                throw new InvalidOperationException();

            try
            {
                var pr = await m_GitHubClient.PullRequest.Get(owner, repo, id);
                return new Uri(pr.HtmlUrl);
            }
            catch (NotFoundException)
            {
                return null;
            }
        }
    }
}
