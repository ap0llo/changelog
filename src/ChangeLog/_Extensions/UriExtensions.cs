using System;

namespace Grynwald.ChangeLog
{
    internal static class UriExtensions
    {
        public static bool IsWebLink(this Uri uri) => uri.Scheme.ToLower() switch
        {
            "http" => true,
            "https" => true,
            _ => false
        };
    }
}
