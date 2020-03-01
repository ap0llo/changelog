using System;
using System.Diagnostics.CodeAnalysis;

namespace ChangeLogCreator.ConventionalCommits
{
    /// <summary>
    /// Encapsulates a commit message's header.  
    /// </summary>
    /// <remarks>
    /// The header encapsulates all information provided in the commit message subject and is mandatory for all commit messages.
    /// </remarks>
    /// <seealso href="https://www.conventionalcommits.org">Conventional Commits</see>
    /// <seealso cref="CommitMessage"/>
    public sealed class CommitMessageHeader : IEquatable<CommitMessageHeader>
    {
        /// <summary>
        /// Gets the type of change, e.g. <c>feat</c> or <c>fix</c>.
        /// </summary>
        public CommitType Type { get; }

        /// <summary>
        /// The optional scope of the change
        /// </summary>
        public string? Scope { get; }

        /// <summary>
        /// Gets whether the commit was marked as breaking change.
        /// </summary>
        /// <remarks>
        /// A commit can be marked as breaking change including by a <c>!</c> after the scope.
        /// <para>
        /// Note: Breaking changes might also be indicated using a "BREAKING CHANGE" footer.
        /// This property only indicates if the change was marked as breaking change in the header.
        /// </para>
        /// </remarks>
        public bool IsBreakingChange { get; }

        /// <summary>
        /// Gets the description of the change.
        /// </summary>
        /// <remarks>
        /// The description is a short summary of the change.
        /// <para>
        /// Note: The value of this property does not include the type of change (see <see cref="Type"/>).
        /// </para>
        /// </remarks>
        public string Description { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="CommitMessageHeader"/>
        /// </summary>
        /// <param name="type">The type of the change (see <see cref="Type"/> property).</param>
        /// <param name="description">The change's description (see <see cref="Description"/> property).</param>
        public CommitMessageHeader(CommitType type, string description) : this(type, description, null, false)
        { }


        /// <summary>
        /// Initializes a new instance of <see cref="CommitMessageHeader"/>
        /// </summary>
        /// <param name="type">The type of the change (see <see cref="Type"/> property).</param>
        /// <param name="description">The change's description (see <see cref="Description"/> property).</param>
        /// <param name="scope">The (optional) scope of the change (see <see cref="Scope"/> property).</param>
        public CommitMessageHeader(CommitType type, string description, string? scope) : this(type, description, scope, false)
        { }

        /// <summary>
        /// Initializes a new instance of <see cref="CommitMessageHeader"/>
        /// </summary>
        /// <param name="type">The type of the change (see <see cref="Type"/> property).</param>
        /// <param name="description">The change's description (see <see cref="Description"/> property).</param>
        /// <param name="scope">The (optional) scope of the change (see <see cref="Scope"/> property).</param>
        /// <param name="isBreakingChange">Indicates whether the change is marked as a breaking change (see <see cref="IsBreakingChange"/> property).</param>
        public CommitMessageHeader(CommitType type, string description, string? scope, bool isBreakingChange)
        {            
            if (String.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Value must not be null or whitespace", nameof(description));

            Type = type;
            Scope = scope;
            IsBreakingChange = isBreakingChange;
            Description = description;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = StringComparer.OrdinalIgnoreCase.GetHashCode(Type) * 397;
                hash ^= (Scope == null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(Scope));
                hash ^= StringComparer.Ordinal.GetHashCode(Description);
                return hash;
            }
        }

        public override bool Equals(object? obj) => Equals(obj as CommitMessage);

        public bool Equals([AllowNull] CommitMessageHeader other) =>
            other != null &&
            StringComparer.OrdinalIgnoreCase.Equals(Type, other.Type) &&
            StringComparer.OrdinalIgnoreCase.Equals(Scope, other.Scope) &&
            IsBreakingChange == other.IsBreakingChange &&
            StringComparer.Ordinal.Equals(Description, other.Description);
    }
}
