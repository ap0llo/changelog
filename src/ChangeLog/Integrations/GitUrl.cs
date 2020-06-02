using System;
using System.Diagnostics.CodeAnalysis;

namespace Grynwald.ChangeLog.Integrations
{
    public static class GitUrl
    {
        private static readonly char[] s_ScpUrlSplitChars = new[] { ':' };


        public static bool TryGetUri(string url, [NotNullWhen(true)] out Uri? uri)
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


        private static bool TryParseScpUrl(string url, [NotNullWhen(true)] out Uri? sshUri)
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
    }
}
