using System;
using System.Diagnostics.CodeAnalysis;

namespace Grynwald.ChangeLog.Integrations.GitLab
{
    public static class GitLabUrlParser
    {
        public static GitLabProjectInfo ParseRemoteUrl(string url)
        {
            if (TryParseRemoteUrl(url, out var projectInfo, out var errorMessage))
            {
                return projectInfo;
            }
            else
            {
                throw new ArgumentException(errorMessage, nameof(url));
            }
        }

        public static bool TryParseRemoteUrl(string url, [NotNullWhen(true)]out GitLabProjectInfo? projectInfo) =>
            TryParseRemoteUrl(url, out projectInfo, out var _);


        private static bool TryParseRemoteUrl(string url, [NotNullWhen(true)]out GitLabProjectInfo? projectInfo, [NotNullWhen(false)]out string? errorMessage)
        {
            projectInfo = null;
            errorMessage = null;

            if (String.IsNullOrWhiteSpace(url))
            {
                errorMessage = "Value must not be null or empty";
                return false;
            }

            if (!GitUrl.TryGetUri(url, out var uri))
            {
                errorMessage = $"Value '{url}' is not a valid uri";
                return false;
            }

            switch (uri.Scheme.ToLower())
            {
                case "http":
                case "https":
                case "ssh":
                    var projectPath = uri.AbsolutePath.Trim('/');
                    projectPath = projectPath.RemoveSuffix(".git");
                    projectInfo = new GitLabProjectInfo(uri.Host, projectPath);
                    return true;

                default:
                    errorMessage = $"Cannot parse '{url}' as GitLab url: Unsupported scheme '{uri.Scheme}'";
                    return false;
            }
        }
    }
}
