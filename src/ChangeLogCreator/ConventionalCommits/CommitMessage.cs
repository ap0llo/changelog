using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace ChangeLogCreator.ConventionalCommits
{
    /// <summary>
    /// Encapsulates a parsed commit message following the <see href="https://www.conventionalcommits.org">Conventional Commits </see> format.
    /// </summary>
    /// <seealso href="https://www.conventionalcommits.org">Conventional Commits</see>
    public sealed class CommitMessage : IEquatable<CommitMessage>
    {
        /// <summary>
        /// Gets the commit message's header (i.e. the main information provided in the commit subject).
        /// </summary>
        public CommitMessageHeader Header { get; }

        /// <summary>
        /// Gets the paragraphs of the message's body.
        /// A commit message body is optional, if no body was provided, returns an empty list.
        /// </summary>
        public IReadOnlyList<string> Body { get; }

        /// <summary>
        /// Gets the message's footers.
        /// Commit message footers are optional, if no footer was provided, returns an empty list.
        /// </summary>
        public IReadOnlyList<CommitMessageFooter> Footers { get; }


        /// <summary>
        /// Initializes a new instance of <see cref="CommitMessage"/> with an empty body and no footers
        /// </summary>
        /// <param name="header">The commit message's header (the value of the <see cref="Header"/> property).</param>
        public CommitMessage(CommitMessageHeader header) : this(header, Array.Empty<string>(), Array.Empty<CommitMessageFooter>())
        { }

        /// <summary>
        /// Initializes a new instance of <see cref="CommitMessage"/> without footers
        /// </summary>
        /// <param name="header">The commit message's header (the value of the <see cref="Header"/> property).</param>
        /// <param name="body">The commit message's body (the value of the <see cref="Body"/> property).</param>
        public CommitMessage(CommitMessageHeader header, IReadOnlyList<string> body) : this(header, body, Array.Empty<CommitMessageFooter>())
        { }

        /// <summary>
        /// Initializes a new instance of <see cref="CommitMessage"/>
        /// </summary>
        /// <param name="header">The commit message's header (the value of the <see cref="Header"/> property).</param>
        /// <param name="body">The commit message's body (the value of the <see cref="Body"/> property).</param>
        /// <param name="footers">The commit message's footers (the value of the <see cref="Footers"/> property).</param>
        public CommitMessage(CommitMessageHeader header, IReadOnlyList<string> body, IReadOnlyList<CommitMessageFooter> footers)
        {
            Header = header ?? throw new ArgumentNullException(nameof(header));
            Body = body ?? throw new ArgumentNullException(nameof(body));
            Footers = footers ?? throw new ArgumentNullException(nameof(footers));
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public override bool Equals(object? obj) => Equals(obj as CommitMessage);

        /// <inheritdoc />
        public bool Equals([AllowNull] CommitMessage other) =>
            other != null &&
            Header.Equals(other.Header) &&
            Body.SequenceEqual(other.Body) &&
            Footers.SequenceEqual(other.Footers);
    }

}
