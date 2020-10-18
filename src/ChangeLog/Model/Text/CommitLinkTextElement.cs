using System;
using Grynwald.ChangeLog.Git;

namespace Grynwald.ChangeLog.Model.Text
{
    /// <summary>
    /// Represents a link to a git commit
    /// </summary>
    public sealed class CommitLinkTextElement : TextElement
    {
        /// <summary>
        /// Gets the commit's id
        /// </summary>
        public GitId Id { get; }


        /// <summary>
        /// Initializes a new instance of <see cref="CommitLink"/>
        /// </summary>
        /// <param name="id">The commit's id</param>
        public CommitLinkTextElement(string text, GitId id) : base(text)
        {
            if (id.IsNull)
                throw new ArgumentException("Commit id must not be empty", nameof(id));

            Id = id;
        }
    }
}
