using System;
using System.Diagnostics.CodeAnalysis;

namespace ChangeLogCreator.Integrations.GitHub
{
    internal static class GitHubUrlParser
    {
        private static readonly char[] s_ScpUrlSplitChars = new[] { ':' };


        public static GitHubProjectInfo ParseRemoreUrl(string url)
        {
            if (String.IsNullOrWhiteSpace(url))
                throw new ArgumentException("Value must not be null or empty", nameof(url));


            if (!TryParseUrl(url, out var uri))
            {
                throw new ArgumentException($"Value '{url}' is not a valid uri", nameof(url));
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
                        throw new ArgumentException($"Cannot parse '{url}' as GitHub url", nameof(url));

                    return new GitHubProjectInfo(uri.Host, ownerAndRepo[0], ownerAndRepo[1]);

                default:
                    throw new ArgumentException($"Cannot parse '{url}' as GitHub url: Unsupported scheme '{uri.Scheme}'", nameof(url));
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
