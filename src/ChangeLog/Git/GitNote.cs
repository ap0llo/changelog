using System;
using System.Diagnostics.CodeAnalysis;

namespace Grynwald.ChangeLog.Git
{
    public sealed class GitNote : IEquatable<GitNote>
    {
        /// <summary>
        /// The id of the target object of the note (typically a commit)
        /// </summary>
        public GitId Target { get; }

        /// <summary>
        /// The "namespace" of the note (i.e. the abrreviated "notes ref").
        /// Defaults to <c>commits</c>
        /// </summary>
        public string Namespace { get; }

        /// <summary>
        /// The note's message
        /// </summary>
        public string Message { get; }


        public GitNote(GitId target, string @namespace, string message)
        {
            if (target.IsNull)
                throw new ArgumentException("Object id must not be empty", nameof(target));

            if (String.IsNullOrWhiteSpace(@namespace))
                throw new ArgumentException("Value must not be null or whitespace", nameof(@namespace));

            Target = target;
            Namespace = @namespace;
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }


        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hash = Target.GetHashCode() * 397;
                hash ^= StringComparer.Ordinal.GetHashCode(Namespace);
                hash ^= StringComparer.Ordinal.GetHashCode(Message);
                return hash;
            }
        }

        /// <inheritdoc />
        public override bool Equals(object? obj) => Equals(obj as GitNote);

        /// <inheritdoc />
        public bool Equals([AllowNull] GitNote other)
        {
            if (other is null)
                return false;

            return Target.Equals(other.Target) &&
                StringComparer.Ordinal.Equals(Namespace, other.Namespace) &&
                StringComparer.Ordinal.Equals(Message, other.Message);
        }
    }
}
