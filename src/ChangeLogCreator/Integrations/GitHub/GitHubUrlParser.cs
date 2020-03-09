using System;
using System.Diagnostics.CodeAnalysis;

namespace ChangeLogCreator.Integrations.GitHub
{
    internal static class GitHubUrlParser
    {
        private static readonly char[] s_ScpUrlSplitChars = new[] { ':' };


        public static GitHubProjectInfo ParseRemoteUrl(string url)
        {
            if(TryParseRemoteUrl(url, out var projectInfo, out var errorMessage))
            {
                return projectInfo;
            }
            else
            {
                throw new ArgumentException(errorMessage, nameof(url));
            }
        }

        public static bool TryParseRemoteUrl(string url, [NotNullWhen(true)]out GitHubProjectInfo? projectInfo) => TryParseRemoteUrl(url, out projectInfo, out var _);


        private static bool TryParseRemoteUrl(string url, [NotNullWhen(true)]out GitHubProjectInfo? projectInfo, [NotNullWhen(false)]out string? errorMessage)
        {
            projectInfo = null;
            errorMessage = null;

            if (String.IsNullOrWhiteSpace(url))
            {
                errorMessage = "Value must not be null or empty";
                return false;
            }


            if (!TryParseUrl(url, out var uri))
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
                    path = RemoveSuffix(path, ".git");

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

        private static bool TryParseUrl(string url, [NotNullWhen(true)]out Uri? uri)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out uri))
            {
                return true;
            }
            else
            {
                return TryParseScpUrl(url, out uri);
            }
        }

        private static bool TryParseScpUrl(string url, [NotNullWhen(true)]out Uri? sshUri)
        {
            // Parse a scp-format git url: e.g. git@github.com:ap0llo/changelog-creator.git

            var fragments = url.Split(s_ScpUrlSplitChars, StringSplitOptions.RemoveEmptyEntries);
            if (fragments.Length != 2)
            {
                sshUri = default;
                return false;
            }

            var userNameAndHost = fragments[0];
            var path = fragments[1].TrimStart('/');

            return Uri.TryCreate($"ssh://{userNameAndHost}/{path}", UriKind.Absolute, out sshUri);
        }

        private static string RemoveSuffix(string value, string suffix)
        {
            return value.EndsWith(suffix)
                ? value[..^suffix.Length]
                : value;
        }

    }
}
