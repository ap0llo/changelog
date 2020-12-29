using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

namespace Grynwald.ChangeLog.Integrations.GitLab
{
    /// <summary>
    /// Represents a reference to an item (Issue, Merge Request of Milestone) on GitLab
    /// </summary>
    /// <seealso href="https://docs.gitlab.com/ee/user/markdown.html#special-gitlab-references">GitLab Reference (GitLab documentation)</seealso>
    public sealed class GitLabReference : IEquatable<GitLabReference>
    {
        private const RegexOptions s_RegexOptions = RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled;
        private static readonly Regex s_GitLabReferencePattern = new Regex("^((?<namespace>[A-Z0-9-_./]+)/)?(?<project>[A-Z0-9-_.]+)?(?<type>(#|!|%))(?<id>\\d+)$", s_RegexOptions);


        /// <summary>
        /// Gets the project the referenced item belongs to.
        /// </summary>
        public GitLabProjectInfo Project { get; }

        /// <summary>
        /// Gets the type (Issue, Merge Request or Milestone) of the referenced item
        /// </summary>
        public GitLabReferenceType Type { get; }

        /// <summary>
        /// Gets the referenced item's number
        /// </summary>
        public int Id { get; }


        /// <summary>
        /// Initializes a new instance of <see cref="GitLabReference"/>
        /// </summary>
        /// <param name="project">The project the referenced item belong to.</param>
        /// <param name="type">The type of the referenced item.</param>
        /// <param name="id">The referenced item's id.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="id"/> is 0 or a negative number</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="project"/> is <c>null</c></exception>
        public GitLabReference(GitLabProjectInfo project, GitLabReferenceType type, int id)
        {
            if (id < 1)
                throw new ArgumentOutOfRangeException(nameof(id));

            Project = project ?? throw new ArgumentNullException(nameof(project));
            Type = type;
            Id = id;
        }


        /// <inheritdoc /> 
        public override bool Equals(object? obj) => Equals(obj as GitLabReference);

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(Project, Type, Id);

        /// <inheritdoc />
        public bool Equals(GitLabReference? other)
        {
            return other is not null &&
                Id == other.Id &&
                Type == other.Type &&
                Project.Equals(other.Project);
        }

        /// <inheritdoc />
        public override string ToString() => ToString(GitLabReferenceFormat.Full);

        /// <summary>
        /// Converts the reference to a string using the specified format
        /// </summary>
        public string ToString(GitLabReferenceFormat format)
        {
            var stringBuilder = new StringBuilder();

            if (format == GitLabReferenceFormat.Full)
            {
                stringBuilder.Append(Project.ProjectPath);
            }
            else if (format == GitLabReferenceFormat.ProjectAndItem)
            {
                stringBuilder.Append(Project.Project);
            }

            stringBuilder.Append(Type switch
            {
                GitLabReferenceType.Issue => "#",
                GitLabReferenceType.MergeRequest => "!",
                GitLabReferenceType.Milestone => "%",
                _ => throw new NotImplementedException()
            });
            stringBuilder.Append(Id);

            return stringBuilder.ToString().ToLower();
        }

        /// <summary>
        /// Tries to parse the specified input as GitLab reference.
        /// </summary>
        /// <remarks>
        /// Attempts to parse the specified reference.
        /// When the reference does not include the project name and namespace, the missing values are taken from <paramref name="currentProject"/>.
        /// </remarks>
        /// <param name="input">The reference to parse</param>
        /// <param name="currentProject">The current project in which's context to parse the reference</param>
        /// <param name="result">When parsing succeeds contains the parsed reference.</param>
        /// <returns>Returns <c>true</c> when the reference could be parsed, otherwise returns <c>true</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="currentProject"/> is <c>null</c>.</exception>
        public static bool TryParse(string input, GitLabProjectInfo currentProject, [NotNullWhen(true)] out GitLabReference? result)
        {
            if (currentProject is null)
                throw new ArgumentNullException(nameof(currentProject));

            if (String.IsNullOrWhiteSpace(input))
            {
                result = default;
                return false;
            }

            input = input.Trim();

            var match = s_GitLabReferencePattern.Match(input);

            if (match.Success)
            {
                var idString = match.Groups["id"].ToString();
                if (Int32.TryParse(idString, out var id))
                {
                    var projectNamespace = match.Groups["namespace"].Value;
                    var projectName = match.Groups["project"].Value;
                    var typeString = match.Groups["type"].Value;

                    GitLabReferenceType type;
                    switch (typeString)
                    {
                        case "#":
                            type = GitLabReferenceType.Issue;
                            break;
                        case "!":
                            type = GitLabReferenceType.MergeRequest;
                            break;
                        case "%":
                            type = GitLabReferenceType.Milestone;
                            break;

                        default:
                            result = default;
                            return false;
                    }


                    // no project name or namespace => reference within the current project
                    if (String.IsNullOrEmpty(projectNamespace) && String.IsNullOrEmpty(projectName))
                    {
                        result = new(currentProject, type, id);
                        return true;
                    }
                    // project name without namespace => reference to another project within the same namespace
                    else if (String.IsNullOrEmpty(projectNamespace) && !String.IsNullOrEmpty(projectName))
                    {
                        result = new(new(currentProject.Host, currentProject.Namespace, projectName), type, id);
                        return true;
                    }
                    // namespace and project name found => full reference
                    else if (!String.IsNullOrEmpty(projectNamespace) && !String.IsNullOrEmpty(projectName))
                    {
                        result = new(new(currentProject.Host, projectNamespace, projectName), type, id);
                        return true;
                    }
                }
            }

            result = default;
            return false;
        }
    }
}
