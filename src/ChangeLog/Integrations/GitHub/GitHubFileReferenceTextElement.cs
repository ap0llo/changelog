using System;
using Grynwald.ChangeLog.Model.Text;

namespace Grynwald.ChangeLog.Integrations.GitHub
{
    /// <summary>
    /// A <see cref="ITextElement"/> that references a file in a GitHub repository
    /// </summary>
    internal sealed class GitHubFileReferenceTextElement : ITextElement, IWebLinkTextElement
    {
        /// <inheritdoc />
        public string Text { get; }

        /// <inheritdoc />
        public TextStyle Style => TextStyle.None;

        /// <inheritdoc />
        public Uri Uri { get; }

        /// <summary>
        /// Gets the relative path of the file within the GitHub repository.
        /// </summary>
        public string RelativePath { get; }


        /// <summary>
        /// Initializes a new instance of <see cref="GitHubReferenceTextElement"/>.
        /// </summary>
        public GitHubFileReferenceTextElement(string text, Uri uri, string relativePath)
        {
            if (String.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Value must not be null or whitespace", nameof(text));

            if (String.IsNullOrWhiteSpace(relativePath))
                throw new ArgumentException("Value must not be null or whitespace", nameof(relativePath));

            Text = text;
            Uri = uri ?? throw new ArgumentNullException(nameof(uri));
            RelativePath = relativePath;
        }
    }
}
