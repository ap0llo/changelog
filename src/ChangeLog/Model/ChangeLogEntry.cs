using System;
using System.Collections.Generic;
using System.Linq;
using Grynwald.ChangeLog.ConventionalCommits;
using Grynwald.ChangeLog.Git;

namespace Grynwald.ChangeLog.Model
{
    public class ChangeLogEntry
    {
        public DateTime Date { get; }

        public CommitType Type { get; }

        public string? Scope { get; } // TODO: Use a custom type instead of string?

        /// <summary>
        /// Gets whether this change contains a breaking change.
        /// </summary>
        public bool ContainsBreakingChanges { get; }

        public string Summary { get; }

        public IReadOnlyList<string> Body { get; }

        public GitId Commit { get; }

        public Uri? CommitWebUri { get; set; }

        /// <summary>
        /// Gets all footers (excluding "breaking change" footers)
        /// </summary>
        public IReadOnlyList<ChangeLogEntryFooter> Footers { get; }

        /// <summary>
        /// Gets the description of breaking changes of this changelog entry.        
        /// </summary>
        /// <remarks>
        /// Returns all "breaking change" descriptions for this changelog entry.
        /// If no description were provided but the entry changelog entry was marked as a breaking change,
        /// a empty enumerable is returned.
        /// </remarks>
        public IEnumerable<string> BreakingChangeDescriptions { get; }


        public ChangeLogEntry(
            DateTime date,
            CommitType type,
            string? scope,
            bool isBreakingChange,
            string summary,
            IReadOnlyList<string> body,
            IReadOnlyList<ChangeLogEntryFooter> footers,
            IReadOnlyList<string> breakingChangeDescriptions,
            GitId commit)
        {
            if (footers is null)
                throw new ArgumentNullException(nameof(footers));

            if (breakingChangeDescriptions is null)
                throw new ArgumentNullException(nameof(breakingChangeDescriptions));

            Date = date;
            Type = type;
            Scope = scope;
            ContainsBreakingChanges = isBreakingChange || breakingChangeDescriptions.Count > 0;
            Summary = summary ?? throw new ArgumentNullException(nameof(summary));
            Body = body ?? throw new ArgumentNullException(nameof(body));
            Footers = footers;
            BreakingChangeDescriptions = breakingChangeDescriptions;
            Commit = commit;
        }

        public static ChangeLogEntry FromCommitMessage(GitCommit commit, CommitMessage commitMessage)
        {
            var breakingChangeDescriptions = commitMessage.Footers
                .Where(x => x.Name.IsBreakingChange)
                .Select(x => x.Value)
                .ToArray();

            var footers = commitMessage.Footers
                .Where(x => !x.Name.IsBreakingChange)
                .Select(ChangeLogEntryFooter.FromCommitMessageFooter)
                .ToArray();

            return new ChangeLogEntry(
                date: commit.Date,
                type: commitMessage.Header.Type,
                scope: commitMessage.Header.Scope,
                isBreakingChange: commitMessage.Header.IsBreakingChange,
                summary: commitMessage.Header.Description,
                body: commitMessage.Body,
                footers: footers,
                breakingChangeDescriptions: breakingChangeDescriptions,
                commit: commit.Id
            );
        }


    }
}


