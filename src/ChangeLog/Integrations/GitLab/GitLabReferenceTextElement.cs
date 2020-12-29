using System;
using Grynwald.ChangeLog.Model.Text;

namespace Grynwald.ChangeLog.Integrations.GitLab
{
    /// <summary>
    /// A <see cref="ITextElement"/> that references a GitLab Issue, Merge Request or Milestone
    /// </summary>
    internal sealed class GitLabReferenceTextElement : ITextElement, IWebLinkTextElement, INormalizedTextElement
    {
        /// <inheritdoc />
        public string Text { get; }

        /// <inheritdoc />
        public TextStyle Style => TextStyle.None;

        /// <inheritdoc />
        public Uri Uri { get; }

        /// <inheritdoc />  
        public string NormalizedText
        {
            get
            {
                if (Reference.Project.Equals(CurrentProject))
                    return Reference.ToString(GitLabReferenceFormat.Item);

                if (StringComparer.OrdinalIgnoreCase.Equals(CurrentProject.Namespace, Reference.Project.Namespace))
                    return Reference.ToString(GitLabReferenceFormat.ProjectAndItem);

                return Reference.ToString(GitLabReferenceFormat.Full);
            }
        }

        /// <inheritdoc />
        public TextStyle NormalizedStyle => TextStyle.None;

        /// <summary>
        /// Gets the GitLab project context for the reference.
        /// </summary>
        /// <remarks>
        /// The current project influences the text returned by <see cref="NormalizedText"/>.
        /// For referenced items in the same project, only the item type and id are returned (e.g. <c>#23</c> for issue 23).
        /// When the referenced item is in a different project within the same namespace, the project name is returned as well (e.g. <c>project#23</c>).
        /// When the referenced item is in a different namesapce, the full reference is returned (e.g. <c>namespace/project#23</c>).
        /// </remarks>
        public GitLabProjectInfo CurrentProject { get; }

        /// <summary>
        /// Gets the <see cref="GitLabReference"/> for the referenced item
        /// </summary>
        public GitLabReference Reference { get; }


        /// <summary>
        /// Initializes a new instance of <see cref="GitHubReferenceTextElement"/>.
        /// </summary>
        public GitLabReferenceTextElement(string text, Uri uri, GitLabProjectInfo currentProject, GitLabReference reference)
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
