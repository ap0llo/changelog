using System;
using Grynwald.ChangeLog.Git;

namespace Grynwald.ChangeLog.Model
{
    /// <summary>
    /// Represents a link to a git commit
    /// </summary>
    public sealed class CommitLink : ILink
    {
        /// <summary>
        /// Gets the commit's id
        /// </summary>
        public GitId Id { get; }


        /// <summary>
        /// Initializes a new instance of <see cref="CommitLink"/>
        /// </summary>
        /// <param name="id">The commit's id</param>
        public CommitLink(GitId id)
        {
            if (id.IsNull)
                throw new ArgumentException("Commit id must not be empty", nameof(id));

            Id = id;
        }
    }
}
