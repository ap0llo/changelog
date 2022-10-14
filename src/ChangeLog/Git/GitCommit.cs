using System;

namespace Grynwald.ChangeLog.Git
{

    public sealed class GitCommit : IEquatable<GitCommit>
    {
        public GitId Id { get; }

        public string CommitMessage { get; }

        public DateTime Date { get; }

        public GitAuthor Author { get; }


        public GitCommit(GitId id, string commitMessage, DateTime date, GitAuthor author)
        {
            if (id.IsNull)
                throw new ArgumentException("Commit id must not be empty", nameof(id));

            Id = id;
            CommitMessage = commitMessage ?? throw new ArgumentNullException(nameof(commitMessage));
            Date = date;
            Author = author ?? throw new ArgumentNullException(nameof(author));
        }


        public override int GetHashCode() => Id.GetHashCode();

        public override bool Equals(object? obj) => Equals(obj as GitCommit);

        public bool Equals(GitCommit? other) =>
            other is not null &&
            Id.Equals(other.Id) &&
            StringComparer.Ordinal.Equals(CommitMessage, other.CommitMessage) &&
            Date == other.Date &&
            Author.Equals(other.Author);
    }
}
