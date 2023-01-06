using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Grynwald.ChangeLog.Utilities
{
    /// <summary>
    /// Simple regex-based parsed for Markdown links (e.g. <c>[Some Link](https://example.com)</c>).
    /// </summary>
    /// <seealso href="https://spec.commonmark.org/0.30/#links">Links (CommonMark Spec, version 0.30)</seealso>
    internal static partial class MarkdownLinkParser
    {
#if NET7_0_OR_GREATER
        [GeneratedRegex("^\\s*\\[(?<text>.*)\\]\\((?<destination>.+)\\)\\s*$", RegexOptions.Singleline)]
        private static partial Regex MarkdownLinkRegex();

        // TODO: This field can be removed and usages can be replaced by a call to MarkdownLinkRegex(), once .NET 6 support is no removed
        private static readonly Regex s_MarkdownLinkRegex = MarkdownLinkRegex();
#else
        private static readonly Regex s_MarkdownLinkRegex = new("^\\s*\\[(?<text>.*)\\]\\((?<destination>.+)\\)\\s*$", RegexOptions.Singleline);
#endif

        public static bool TryParseMarkdownLink(string markdown, out string? text, [NotNullWhen(true)] out string? destination)
        {
            if (s_MarkdownLinkRegex.Match(markdown) is { Success: true } match)
            {
                destination = match.Groups["destination"].Value;
                text = match.Groups["text"].Value;
                if (!String.IsNullOrWhiteSpace(destination))
                {
                    return true;
                }
            }

            text = null;
            destination = null;
            return false;
        }
    }
}
