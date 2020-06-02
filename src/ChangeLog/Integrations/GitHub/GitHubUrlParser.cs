using System;
using System.Diagnostics.CodeAnalysis;

namespace Grynwald.ChangeLog.Integrations.GitHub
{
    internal static class GitHubUrlParser
    {
        public static GitHubProjectInfo ParseRemoteUrl(string url)
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

        public static bool TryParseRemoteUrl(string url, [NotNullWhen(true)] out GitHubProjectInfo? projectInfo) => TryParseRemoteUrl(url, out projectInfo, out var _);


        private static bool TryParseRemoteUrl(string url, [NotNullWhen(true)] out GitHubProjectInfo? projectInfo, [NotNullWhen(false)] out string? errorMessage)
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
                    var path = uri.AbsolutePath.Trim('/');
                    path = path.RemoveSuffix(".git");

                    var ownerAndRepo = path.Split('/');
                    if (ownerAndRepo.Length != 2)
                    {
                        errorMessage = $"Cannot parse '{url}' as GitHub url";
                        return false;
                    }

                    projectInfo = new GitHubProjectInfo(uri.Host, ownerAndRepo[0], ownerAndRepo[1]);
                    return true;

                default:
                    errorMessage = $"Cannot parse '{url}' as GitHub url: Unsupported scheme '{uri.Scheme}'";
                    return false;
            }
        }
    }
}
