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


        protected SingleVersionChangeLog GetSingleVersionChangeLog(string version, string commitId = null)
        {
            return new SingleVersionChangeLog(
                new VersionInfo(
                    SemanticVersion.Parse(version),
                    commitId == null ? NextGitId() : new GitId(commitId)
                ));
        }

        protected ChangeLogEntry GetChangeLogEntry(
            DateTime? date = null,
            string? type = null,
            string? scope = null,
            string? summary = null,
            IReadOnlyList<string>? body = null,
            string? commit = null)
        {

            return new ChangeLogEntry(
                date: date ?? NextCommitDate(),
                type: new CommitType(type ?? "feat"),
                scope: scope,
                summary: summary ?? "Example Summary",
                body: body ?? Array.Empty<string>(),
                commit: commit == null ? NextGitId() : new GitId(commit));
        }


        protected GitCommit GetGitCommit(string? id = null, string? commitMessage = null)
        {
            return new GitCommit(
                id: id == null ? NextGitId(): new GitId(id),
                commitMessage: commitMessage ?? "",
                date: new DateTime(),
                author: new GitAuthor("Someone", "someone@example.com")
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
