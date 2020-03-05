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
        protected SingleVersionChangeLog GetSingleVersionChangeLog(string version, string commitId = null)
        {
            return new SingleVersionChangeLog(
                new VersionInfo(
                    SemanticVersion.Parse(version),
                    new GitId(commitId ?? "00")
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
                date: date ?? DateTime.Now,
                type: new CommitType(type ?? "feat"),
                scope: scope,
                summary: summary ?? "Example Summary",
                body: body ?? Array.Empty<string>(),
                commit: new GitId(commit ?? "0000"));
        }


        protected GitCommit GetGitCommit(string? id = null, string? commitMessage = null)
        {
            return new GitCommit(
                id: new GitId(id ?? "0000"),
                commitMessage: commitMessage ?? "",
                date: new DateTime(),
                author: new GitAuthor("Someone", "someone@example.com")
            );
        }
    }
}
