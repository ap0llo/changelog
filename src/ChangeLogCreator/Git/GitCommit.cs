using System;
using System.Diagnostics.CodeAnalysis;

namespace ChangeLogCreator.Git
{

    public sealed class GitCommit : IEquatable<GitCommit>
    {
        public string Id { get; }

        public string CommitMessage { get; }

        public DateTime Date { get; }

        public GitAuthor Author { get; }


        public GitCommit(string id, string commitMessage, DateTime date, GitAuthor author)
        {
            if (String.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Value must not be null or whitespace", nameof(id));

            Id = id;
            CommitMessage = commitMessage ?? throw new ArgumentNullException(nameof(commitMessage));
            Date = date;
            Author = author ?? throw new ArgumentNullException(nameof(author));
        }


        public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Id);

        public override bool Equals(object? obj) => Equals(obj as GitCommit);

        public bool Equals([AllowNull] GitCommit other) =>
            other != null &&
            StringComparer.OrdinalIgnoreCase.Equals(Id, other.Id) &&
            StringComparer.Ordinal.Equals(CommitMessage, other.CommitMessage) &&
            Date == other.Date &&
            Author.Equals(other.Author);
    }
}
