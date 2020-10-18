using System;
using Grynwald.ChangeLog.Git;

namespace Grynwald.ChangeLog.Model.Text
{
    /// <summary>
    /// Represents a link to a git commit
    /// </summary>
    public sealed class CommitReferenceTextElement : TextElement
    {
        /// <summary>
        /// Gets the commit's id
        /// </summary>
        public GitId CommitId { get; }


        /// <summary>
        /// Initializes a new instance of <see cref="CommitReferenceTextElement"/>
        /// </summary>
        /// <param name="text">The text element's text</param>
        /// <param name="id">The commit's id</param>
        public CommitReferenceTextElement(string text, GitId id) : base(text)
        {
            if (id.IsNull)
                throw new ArgumentException("Commit id must not be empty", nameof(id));

            CommitId = id;
        }
    }
}
