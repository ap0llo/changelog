using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Grynwald.ChangeLog.Integrations.GitHub
{
    /// <summary>
    /// Represents a reference to a GitHub Pull Request or Issue
    /// </summary>
    /// <see href="https://docs.github.com/en/free-pro-team@latest/github/writing-on-github/autolinked-references-and-urls#issues-and-pull-requests">Autolinked references and URLs (GitHub Docs)</see>
    public sealed class GitHubReference : IEquatable<GitHubReference>
    {
        private const RegexOptions s_RegexOptions = RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled;
        private static readonly IReadOnlyList<Regex> s_GitHubReferencePatterns = new[]
        {
            new Regex("^((?<owner>[A-Z0-9-_.]+)/(?<repo>[A-Z0-9-_.]+))?#(?<id>\\d+)$", s_RegexOptions),
            new Regex("^GH-(?<id>\\d+)$", s_RegexOptions),
        };


        /// <summary>
        /// Gets the project the Pull Request or Issue belongs to
        /// </summary>
        public GitHubProjectInfo Project { get; }

        /// <summary>
        /// Gets the Pull Request or Issue number
        /// </summary>
        public int Id { get; }


        /// <summary>
        /// Initializes a new instance of <see cref="GitHubReference"/>
        /// </summary>
        /// <param name="project">The project the Pull Request of Issue belongs to</param>
        /// <param name="id">The Pull Request or Issue number</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="id"/> is 0 or a negative number</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="project"/> is <c>null</c></exception>
        public GitHubReference(GitHubProjectInfo project, int id)
        {
            if (id < 1)
                throw new ArgumentOutOfRangeException(nameof(id));

            Project = project ?? throw new ArgumentNullException(nameof(project));
            Id = id;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj) => Equals(obj as GitHubReference);

        /// <inheritdoc />
        public bool Equals(GitHubReference? other)
        {
            return other is not null &&
                Project.Equals(other.Project) &&
                Id == other.Id;
        }

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(Project, Id);

        /// <inheritdoc />
        public override string ToString() => ToString(GitHubReferenceFormat.Full);

        /// <summary>
        /// Converts the reference to a string using the specified format
        /// </summary>
        public string ToString(GitHubReferenceFormat format) => format switch
        {
            GitHubReferenceFormat.Full => $"{Project.Owner}/{Project.Repository}#{Id}".ToLower(),
            GitHubReferenceFormat.Minimal => $"#{Id}",
            _ => throw new NotImplementedException()
        };


        /// <summary>
        /// Tries to parse the specified input as GitHub reference.
        /// </summary>
        /// <remarks>
        /// Attempts to parse the specified reference.
        /// When the reference does not include the repository name and owner, these values are taken from <paramref name="currentProject"/>.
        /// </remarks>
        /// <param name="input">The reference to parse</param>
        /// <param name="currentProject">The current project in which's context to parse the reference</param>
        /// <param name="result">When parsing succeeds contains the parsed reference.</param>
        /// <returns>Returns <c>true</c> when the reference could be parsed, otherwise returns <c>true</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="currentProject"/> is <c>null</c>.</exception>
        public static bool TryParse(string input, GitHubProjectInfo currentProject, [NotNullWhen(true)] out GitHubReference? result)
        {
            if (currentProject is null)
                throw new ArgumentNullException(nameof(currentProject));

            if (String.IsNullOrWhiteSpace(input))
            {
                result = default;
                return false;
            }

            input = input.Trim();

            // using every pattern, try to get a issue/PR id from the input text
            foreach (var pattern in s_GitHubReferencePatterns)
            {
                var match = pattern.Match(input);

                if (match.Success)
                {
                    var idString = match.Groups["id"].ToString();
                    if (Int32.TryParse(idString, out var id))
                    {
                        var owner = match.Groups["owner"].Value;
                        var repo = match.Groups["repo"].Value;

                        if (String.IsNullOrEmpty(owner))
                            owner = currentProject.Owner;

                        if (String.IsNullOrEmpty(repo))
                            repo = currentProject.Repository;

                        result = new GitHubReference(new(currentProject.Host, owner, repo), id);
                        return true;
                    }
                }
            }

            result = default;
            return false;
        }
    }
}
