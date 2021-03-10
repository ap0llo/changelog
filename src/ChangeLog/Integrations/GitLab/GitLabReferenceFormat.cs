namespace Grynwald.ChangeLog.Integrations.GitLab
{
    /// <summary>
    /// Enumerates the supported string formats for GitLab reference
    /// </summary>
    /// <seealso cref="GitLabReference"/>
    public enum GitLabReferenceFormat
    {
        /// <summary>
        /// Include project namespace, project name, item type and id
        /// </summary>
        /// <example>
        /// <c>namespace/project#23</c>
        /// </example>
        Full,

        /// <summary>
        /// Include project name, item type and item id (for references within the same namespace)
        /// </summary>
        /// <example>
        /// <c>project#23</c>
        /// </example>
        ProjectAndItem,

        /// <summary>
        /// Include only the item type and id (for references within a project)
        /// </summary>
        /// <example>
        /// <c>#23</c>
        /// </example>
        Item
    }
}
