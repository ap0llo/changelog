using System;
using System.Collections.Generic;
using ChangeLogCreator.Git;

namespace ChangeLogCreator.Model
{
    public class ChangeLogEntry
    {
        public DateTime Date { get; }

        public string Type { get; }

        public string? Scope { get; } //TODO: Use a custom type instead of string?

        public string Summary { get; }

        public IReadOnlyList<string> Body { get; }

        public GitId Commit { get; }

        public ChangeLogEntry(DateTime date, string type, string? scope, string summary, IReadOnlyList<string> body, GitId commit)
        {
            Date = date;
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Scope = scope;
            Summary = summary ?? throw new ArgumentNullException(nameof(summary));
            Body = body ?? throw new ArgumentNullException(nameof(body));
            Commit = commit;
        }

        //TODO: Footers
        //TODO: IsBreakingChange

    }
}


