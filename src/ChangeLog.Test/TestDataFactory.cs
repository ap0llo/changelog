using System;
using System.Collections.Generic;
using System.Linq;
using Grynwald.ChangeLog.ConventionalCommits;
using Grynwald.ChangeLog.Git;
using Grynwald.ChangeLog.Model;
using NuGet.Versioning;

namespace Grynwald.ChangeLog.Test
{
    public class TestDataFactory
    {
        private DateTime m_NextCommitDate = new DateTime(2020, 1, 1);
        private Random m_CommitIdSource = new Random(23);


        public SingleVersionChangeLog GetSingleVersionChangeLog(string version, GitId? commitId = null, params ChangeLogEntry[] entries)
        {
            var changelog = new SingleVersionChangeLog(
                new VersionInfo(
                    NuGetVersion.Parse(version),
                    commitId ?? NextGitId()
                ));

            foreach (var entry in entries)
            {
                changelog.Add(entry);
            }

            return changelog;
        }

        public ChangeLogEntry GetChangeLogEntry(
            DateTime? date = null,
            string? type = null,
            string? scope = null,
            bool? isBreakingChange = null,
            string? summary = null,
            IReadOnlyList<string>? body = null,
            IReadOnlyList<ChangeLogEntryFooter>? footers = null,
            IReadOnlyList<string>? breakingChangeDescriptions = null,
            GitId? commit = null)
        {

            return new ChangeLogEntry(
                date: date ?? NextCommitDate(),
                type: new CommitType(type ?? "feat"),
                scope: scope,
                isBreakingChange: isBreakingChange ?? false,
                summary: summary ?? "Example Summary",
                body: body ?? Array.Empty<string>(),
                footers: footers ?? Array.Empty<ChangeLogEntryFooter>(),
                breakingChangeDescriptions: breakingChangeDescriptions ?? Array.Empty<string>(),
                commit: commit ?? NextGitId());
        }


        public GitCommit GetGitCommit(GitId? id = null, string? commitMessage = null)
        {
            return new GitCommit(
                id: id ?? NextGitId(),
                commitMessage: commitMessage ?? "",
                date: new DateTime(),
                author: new GitAuthor("Someone", "someone@example.com")
            );
        }

        public CommitMessage GetCommitMessage(
            string? type = null,
            string? scope = null,
            string? description = null,
            bool? isBreakingChange = null,
            IReadOnlyList<string>? body = null,
            IReadOnlyList<CommitMessageFooter>? footers = null)
        {
            var header = new CommitMessageHeader(
                type is null ? CommitType.Feature : new CommitType(type),
                description ?? "Example Summary",
                scope,
                isBreakingChange ?? false);

            return new CommitMessage(
                header,
                body ?? Array.Empty<string>(),
                footers ?? Array.Empty<CommitMessageFooter>()
            );
        }


        protected DateTime NextCommitDate()
        {
            var date = m_NextCommitDate;
            m_NextCommitDate = m_NextCommitDate.AddDays(1);
            return date;
        }

        protected GitId NextGitId()
        {
            var shaBytes = new byte[20];
            m_CommitIdSource.NextBytes(shaBytes);

            var fullId = String.Join("", shaBytes.Select(x => x.ToString("x2")));
            var abbreviatedId = fullId.Substring(0, 7);
            var id = new GitId(fullId, abbreviatedId);
            return id;
        }
    }
}
