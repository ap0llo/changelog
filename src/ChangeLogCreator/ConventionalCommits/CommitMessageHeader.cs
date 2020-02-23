using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace ChangeLogCreator.ConventionalCommits
{
    public sealed class CommitMessageHeader : IEquatable<CommitMessageHeader>
    {
        /// <summary>
        /// The type of change, e.g. 'feat' or 'fix'
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// The optional scope of the change
        /// </summary>
        public string? Scope { get; }

        /// <summary>
        /// Indicates whether a breaking changes hint was included in the header 
        /// (breaking changes are indicated by a '!' after the scope)
        /// Note: Breaking changes might also be indicated using a "BREAKING CHANGE" footer.
        /// </summary>
        public bool IsBreakingChange { get; }

        /// <summary>
        /// The description of the change, i.e. the summary.
        /// Value does not include the feature/scope prefix.
        /// </summary>
        public string Description { get; }


        public CommitMessageHeader(string type, string? scope, bool isBreakingChange, string description)
        {
            if (String.IsNullOrWhiteSpace(type))
                throw new ArgumentException("Value must not be null or whitespace", nameof(type));

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
