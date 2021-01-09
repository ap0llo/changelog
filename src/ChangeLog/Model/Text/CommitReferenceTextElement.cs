using System;
using Grynwald.ChangeLog.Git;

namespace Grynwald.ChangeLog.Model.Text
{
    /// <summary>
    /// Represents a link to a git commit
    /// </summary>
    public class CommitReferenceTextElement : ITextElement, INormalizedTextElement
    {
        /// <inheritdoc />
        public string Text { get; }

        /// <inheritdoc />  
        public TextStyle Style => TextStyle.Code;

        /// <inheritdoc />
        public string NormalizedText => CommitId.ToString(abbreviate: true);

        /// <inheritdoc />  
        public TextStyle NormalizedStyle => TextStyle.Code;


        /// <summary>
        /// Gets the commit's id
        /// </summary>
        public GitId CommitId { get; }


        /// <summary>
        /// Initializes a new instance of <see cref="CommitReferenceTextElement"/>
        /// </summary>
        /// <param name="text">The text element's text</param>
        /// <param name="id">The commit's id</param>
        public CommitReferenceTextElement(string text, GitId id)
        {
            if (String.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Value must not be null or whitespace", nameof(text));

            if (id.IsNull)
                throw new ArgumentException("Commit id must not be empty", nameof(id));

            Text = text;
            CommitId = id;
        }
    }
}
