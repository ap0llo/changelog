using System;
using System.Collections.Generic;
using ChangeLogCreator.ConventionalCommits;
using ChangeLogCreator.Git;
using ChangeLogCreator.Model;
using NuGet.Versioning;

namespace ChangeLogCreator.Test
{
    public abstract class TestBase
    {
        private DateTime m_NextCommitDate = new DateTime(2020, 1, 1);
        private int m_NextCommitId = 1000;


        protected SingleVersionChangeLog GetSingleVersionChangeLog(string version, string? commitId = null, params ChangeLogEntry[] entries)
        {
            var changelog = new SingleVersionChangeLog(
                new VersionInfo(
                    SemanticVersion.Parse(version),
                    commitId == null ? NextGitId() : new GitId(commitId)
                ));

            foreach (var entry in entries)
            {
                changelog.Add(entry);
            }

            return changelog;
        }

        protected ChangeLogEntry GetChangeLogEntry(
            DateTime? date = null,
            string? type = null,
            string? scope = null,
            bool? isBreakingChange = null,
            string? summary = null,
            IReadOnlyList<string>? body = null,
            IReadOnlyList<ChangeLogEntryFooter>? footers = null,
            IReadOnlyList<string>? breakingChangeDescriptions = null,
            string? commit = null)
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
                commit: commit == null ? NextGitId() : new GitId(commit));
        }


        protected GitCommit GetGitCommit(string? id = null, string? commitMessage = null)
        {
            return new GitCommit(
                id: id == null ? NextGitId() : new GitId(id),
                commitMessage: commitMessage ?? "",
                date: new DateTime(),
                author: new GitAuthor("Someone", "someone@example.com")
            );
        }

        protected CommitMessage GetCommitMessage(
            string? type = null,
            string? scope = null,
            string? description = null,
            bool? isBreakingChange = null,
            IReadOnlyList<string>? body = null,
            IReadOnlyList<CommitMessageFooter>? footers = null)
        {
            var header = new CommitMessageHeader(
                type == null ? CommitType.Feature : new CommitType(type),
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
            var id = new GitId(m_NextCommitId.ToString("X6"));
            m_NextCommitId += 100;
            return id;
        }

    }
}
