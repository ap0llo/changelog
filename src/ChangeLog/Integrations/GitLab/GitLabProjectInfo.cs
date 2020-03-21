using System;
using System.Diagnostics.CodeAnalysis;

namespace Grynwald.ChangeLog.Integrations.GitLab
{
    public sealed class GitLabProjectInfo : IEquatable<GitLabProjectInfo>
    {
        /// <summary>
        /// The host name of the GitLab server.
        /// </summary>
        public string Host { get; }

        /// <summary>
        /// The GitLab project path (i.e. namespace + repository name).
        /// </summary>
        public string ProjectPath { get; }


        /// <summary>
        /// Initializes a new instance of <see cref="GitLabProjectInfo"/>
        /// </summary>
        /// <param name="host">The host name of the GitLab server.</param>
        /// <param name="projectPath">The GitLab project path (i.e. namespace + repository name).</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="host"/> or <paramref name="projectPath"/> is null or whitespace</exception>
        public GitLabProjectInfo(string host, string projectPath)
        {
            if (String.IsNullOrWhiteSpace(host))
                throw new ArgumentException("Value must not be null or whitespace", nameof(host));

            if (String.IsNullOrWhiteSpace(projectPath))
                throw new ArgumentException("Value must not be null or whitespace", nameof(projectPath));

            Host = host;
            ProjectPath = projectPath;
        }


        /// <inheritdoc />
        public override bool Equals(object? obj) => Equals(obj as GitLabProjectInfo);

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hash = StringComparer.OrdinalIgnoreCase.GetHashCode(Host) * 397;
                hash ^= StringComparer.OrdinalIgnoreCase.GetHashCode(ProjectPath);
                return hash;
            }
        }

        /// <inheritdoc />
        public bool Equals([AllowNull] GitLabProjectInfo other)
        {
            return other != null &&
                StringComparer.OrdinalIgnoreCase.Equals(Host, other.Host) &&
                StringComparer.OrdinalIgnoreCase.Equals(ProjectPath, other.ProjectPath);
        }
    }
}
