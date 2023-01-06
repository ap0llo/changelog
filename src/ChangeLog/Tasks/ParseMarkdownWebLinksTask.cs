using System;
using System.Linq;
using System.Text.RegularExpressions;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Model.Text;
using Grynwald.ChangeLog.Pipeline;
using Microsoft.Extensions.Logging;

namespace Grynwald.ChangeLog.Tasks
{
    /// <summary>
    /// Detects footer values that are web links using the Markdown link syntax (e.g. <c>[Some Link](https://example.com)</c>).
    /// When a valid link is found, the footer's value (see <see cref="ChangeLogEntryFooter.Value"/>) is replaced with a <see cref="WebLinkTextElement"/>.
    /// </summary>
    /// <seealso href="https://spec.commonmark.org/0.30/#links">Links (CommonMark Spec, version 0.30)</seealso>
    [AfterTask(typeof(ParseCommitsTask))]
    [AfterTask(typeof(ParseWebLinksTask))]
    [BeforeTask(typeof(RenderTemplateTask))]
    internal sealed partial class ParseMarkdownWebLinksTask : SynchronousChangeLogTask
    {
#if NET7_0_OR_GREATER
        [GeneratedRegex("^\\s*\\[(?<text>.*)\\]\\((?<destination>.+)\\)\\s*$", RegexOptions.Singleline)]
        private static partial Regex MarkdownLinkRegex();

        // TODO: This field can be removed and usages can be replaced by a call to MarkdownLinkRegex(), once .NET 6 support is no removed
        private static readonly Regex s_MarkdownLinkRegex = MarkdownLinkRegex();
#else
        private static readonly Regex s_MarkdownLinkRegex = new("^\\s*\\[(?<text>.*)\\]\\((?<destination>.+)\\)\\s*$", RegexOptions.Singleline);
#endif

        private readonly ILogger<ParseMarkdownWebLinksTask> m_Logger;


        public ParseMarkdownWebLinksTask(ILogger<ParseMarkdownWebLinksTask> logger)
        {
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        protected override ChangeLogTaskResult Run(ApplicationChangeLog changelog)
        {
            m_Logger.LogInformation("Checking for Markdown links in footers");

            foreach (var changeLogEntry in changelog.ChangeLogs.SelectMany(x => x.AllEntries))
            {
                foreach (var footer in changeLogEntry.Footers)
                {
                    if (footer.Value is PlainTextElement plainText &&
                        s_MarkdownLinkRegex.Match(plainText.Text) is { Success: true } match &&
                        Uri.TryCreate(match.Groups["destination"].Value, UriKind.Absolute, out var destination) &&
                        destination.IsWebLink())
                    {
                        m_Logger.LogDebug($"Detected Markdown link '{plainText.Text}' in footer for commit '{changeLogEntry.Commit.Id}'. Replacing footer value with web link");

                        var linkText = match.Groups["text"].Value;

                        // If link text is empty, e.g. "[](https://example.com", use the destination as text
                        if (String.IsNullOrWhiteSpace(linkText))
                        {
                            linkText = destination.ToString();
                        }

                        footer.Value = new WebLinkTextElement(linkText, destination);
                    }
                }
            }

            return ChangeLogTaskResult.Success;
        }
    }
}
