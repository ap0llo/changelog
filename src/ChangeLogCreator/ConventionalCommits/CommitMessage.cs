using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace ChangeLogCreator.ConventionalCommits
{
    public sealed class CommitMessage : IEquatable<CommitMessage>
    {
        public CommitMessageHeader Header { get; }

        /// <summary>
        /// The paragraphs of the message's body.
        /// Value is an empty list if no body was provided.
        /// </summary>
        public IReadOnlyList<string> Body { get; }

        /// <summary>
        /// The message's footers.
        /// </summary>
        public IReadOnlyList<CommitMessageFooter> Footers { get; }


        public CommitMessage(CommitMessageHeader header, IReadOnlyList<string> body, IReadOnlyList<CommitMessageFooter> footers)
        {
            Header = header ?? throw new ArgumentNullException(nameof(header));
            Body = body ?? throw new ArgumentNullException(nameof(body));
            Footers = footers ?? throw new ArgumentNullException(nameof(footers));
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = Header.GetHashCode() * 397;
                hash ^= Body.Count;
                hash ^= Footers.Count;
                return hash;
            }
        }

        public override bool Equals(object? obj) => Equals(obj as CommitMessage);

        public bool Equals([AllowNull] CommitMessage other) =>
            other != null &&
            Header.Equals(other.Header) && 
            Body.SequenceEqual(other.Body) &&
            Footers.SequenceEqual(other.Footers);
    }

}
