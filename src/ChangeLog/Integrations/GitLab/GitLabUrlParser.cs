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

                    if (String.IsNullOrWhiteSpace(projectPath))
                    {
                        errorMessage = $"Cannot parse '{url}' as GitLab url: Project path is empty";
                        return false;
                    }

                    if (!projectPath.Contains('/'))
                    {
                        errorMessage = $"Cannot parse '{url}' as GitLab url: Invalid project path '{projectPath}'";
                        return false;
                    }

                    var splitIndex = projectPath.LastIndexOf('/');
                    var @namespace = projectPath.Substring(0, splitIndex);
                    var projectName = projectPath.Substring(splitIndex);

                    if (String.IsNullOrWhiteSpace(@namespace))
                    {
                        errorMessage = $"Cannot parse '{url}' as GitLab url: Project namespace is empty";
                        return false;
                    }

                    if (String.IsNullOrWhiteSpace(projectName))
                    {
                        errorMessage = $"Cannot parse '{url}' as GitLab url: Project name is empty";
                        return false;
                    }

                    projectInfo = new GitLabProjectInfo(uri.Host, @namespace, projectName);
                    return true;

                default:
                    errorMessage = $"Cannot parse '{url}' as GitLab url: Unsupported scheme '{uri.Scheme}'";
                    return false;
            }
        }
    }
}
