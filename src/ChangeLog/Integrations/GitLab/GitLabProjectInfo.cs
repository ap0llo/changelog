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
        /// The project namespace (the user or group (incl.subgroups) the project belongs to
        /// </summary>
        public string Namespace { get; }

        /// <summary>
        /// The project name
        /// </summary>
        public string Project { get; }

        /// <summary>
        /// The GitLab project path (i.e. namespace + repository name).
        /// </summary>
        public string ProjectPath => $"{Namespace}/{Project}";


        /// <summary>
        /// Initializes a new instance of <see cref="GitLabProjectInfo"/>
        /// </summary>
        /// <param name="host">The host name of the GitLab server.</param>
        /// <param name="namespace">The GitLab project's namespace (user name or group and subgroup)</param>
        /// <param name="project">The GitLab project's name</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="host"/>, <paramref name="namespace"/> or <paramref name="project"/> is null or whitespace</exception>
        public GitLabProjectInfo(string host, string @namespace, string project)
        {
            if (String.IsNullOrWhiteSpace(host))
                throw new ArgumentException("Value must not be null or whitespace", nameof(host));

            if (String.IsNullOrWhiteSpace(@namespace))
                throw new ArgumentException("Value must not be null or whitespace", nameof(@namespace));

            if (String.IsNullOrWhiteSpace(project))
                throw new ArgumentException("Value must not be null or whitespace", nameof(project));

            Host = host;
            Namespace = @namespace.Trim('/');
            Project = project.Trim('/');
        }


        /// <inheritdoc />
        public override bool Equals(object? obj) => Equals(obj as GitLabProjectInfo);

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hash = StringComparer.OrdinalIgnoreCase.GetHashCode(Host) * 397;
                hash ^= StringComparer.OrdinalIgnoreCase.GetHashCode(Namespace);
                hash ^= StringComparer.OrdinalIgnoreCase.GetHashCode(Project);
                return hash;
            }
        }

        /// <inheritdoc />
        public bool Equals([AllowNull] GitLabProjectInfo other)
        {
            return other != null &&
                StringComparer.OrdinalIgnoreCase.Equals(Host, other.Host) &&
                StringComparer.OrdinalIgnoreCase.Equals(Namespace, other.Namespace) &&
                StringComparer.OrdinalIgnoreCase.Equals(Project, other.Project);
        }
    }
}
