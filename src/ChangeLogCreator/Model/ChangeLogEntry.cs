using System;
using System.Collections.Generic;
using System.Linq;
using ChangeLogCreator.ConventionalCommits;
using ChangeLogCreator.Git;

namespace ChangeLogCreator.Model
{

    public class ChangeLogEntry
    {
        public DateTime Date { get; }

        public CommitType Type { get; }

        public string? Scope { get; } //TODO: Use a custom type instead of string?

        /// <summary>
        /// Gets whether this change contains a breaking change.
        /// </summary>
        public bool ContainsBreakingChanges { get; }

        public string Summary { get; }

        public IReadOnlyList<string> Body { get; }

        public GitId Commit { get; }

        public IReadOnlyList<CommitMessageFooter> AllFooters { get; }

        /// <summary>
        /// Gets the description of breaking changes of this changelog entry.        
        /// </summary>
        /// <remarks>
        /// Returns all "breaking change" descriptions for this changelog entry.
        /// If no description were provided but the entry changelog entry was marked as a breaking change,
        /// a empty enumerable is returned.
        /// </remarks>
        public IEnumerable<string> BreakingChangeDescriptions => AllFooters.Where(x => x.Name == CommitMessageFooterName.BreakingChange).Select(x => x.Value);


        public ChangeLogEntry(
            DateTime date,
            CommitType type,
            string? scope,
            bool isBreakingChange,
            string summary,
            IReadOnlyList<string> body,
            IReadOnlyList<CommitMessageFooter> footers,
            GitId commit)
        {
            if (footers is null)
                throw new ArgumentNullException(nameof(footers));

            Date = date;
            Type = type;
            Scope = scope;
            ContainsBreakingChanges = isBreakingChange || footers.Any(x => x.Name == CommitMessageFooterName.BreakingChange);
            Summary = summary ?? throw new ArgumentNullException(nameof(summary));
            Body = body ?? throw new ArgumentNullException(nameof(body));
            AllFooters = footers;
            Commit = commit;
        }        
    }
}


