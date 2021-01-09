namespace Grynwald.ChangeLog.Integrations.GitHub
{
    /// <summary>
    /// Enumerates the supported string formats for GitHub references
    /// </summary>
    /// <seealso cref="GitHubReference"/>
    public enum GitHubReferenceFormat
    {
        /// <summary>
        /// Include both the Issue or Pull Request number and the repository name and owner
        /// </summary>
        /// <example>
        /// <c>owner/repository#23</c>
        /// </example>
        Full,

        /// <summary>
        /// Include only the Pull Request or Issue number and the <c>#</c> prefix
        /// </summary>
        /// <example>
        /// <c>#23</c>
        /// </example>
        Minimal
    }
}
