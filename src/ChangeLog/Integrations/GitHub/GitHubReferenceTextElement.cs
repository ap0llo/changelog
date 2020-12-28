using System;
using Grynwald.ChangeLog.Model.Text;

namespace Grynwald.ChangeLog.Integrations.GitHub
{
    /// <summary>
    /// A <see cref="ITextElement"/> that references a GitHub Pull Request or Issue
    /// </summary>
    internal sealed class GitHubReferenceTextElement : ITextElement, IWebLinkTextElement, INormalizedTextElement
    {
        /// <inheritdoc />
        public string Text { get; }

        /// <inheritdoc />
        public TextStyle Style => TextStyle.None;

        /// <inheritdoc />
        public Uri Uri { get; }

        /// <inheritdoc />  
        public string NormalizedText =>
            Reference.Project.Equals(CurrentProject)
                ? Reference.ToString(GitHubReferenceFormat.Minimal)
                : Reference.ToString(GitHubReferenceFormat.Full);

        /// <inheritdoc />
        public TextStyle NormalizedStyle => TextStyle.None;

        /// <summary>
        /// Gets the GitHub repository context for the reference.
        /// When the referenced Issue or Pull Request is in the current project,
        /// the reference text (see <see cref="NormalizedText"/>) is only the issue number and <c>#</c>, e.g. <c>#23</c>
        /// </summary>
        public GitHubProjectInfo CurrentProject { get; }

        /// <summary>
        /// Gets the <see cref="GitHubReference"/> for the referenced Issue or Pull Request
        /// </summary>
        public GitHubReference Reference { get; }


        /// <summary>
        /// Initializes a new instance of <see cref="GitHubReferenceTextElement"/>.
        /// </summary>
        public GitHubReferenceTextElement(string text, Uri uri, GitHubProjectInfo currentProject, GitHubReference reference)
        {
            if (String.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Value must not be null or whitespace", nameof(text));

            Text = text;
            Uri = uri ?? throw new ArgumentNullException(nameof(uri));
            CurrentProject = currentProject ?? throw new ArgumentNullException(nameof(currentProject));
            Reference = reference ?? throw new ArgumentNullException(nameof(reference));
        }
    }
}
