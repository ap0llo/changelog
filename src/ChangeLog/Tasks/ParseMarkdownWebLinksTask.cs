using System;
using System.Linq;
using System.Text.RegularExpressions;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Model.Text;
using Grynwald.ChangeLog.Pipeline;
using Grynwald.ChangeLog.Utilities;
using Microsoft.Extensions.Logging;

namespace Grynwald.ChangeLog.Tasks
{
    /// <summary>
    /// Detects footer values that are web links using the Markdown link syntax (e.g. <c>[Some Link](https://example.com)</c>).
    /// When a valid link is found, the footer's value (see <see cref="ChangeLogEntryFooter.Value"/>) is replaced with a <see cref="WebLinkTextElement"/>.
    /// </summary>    
    [AfterTask(typeof(ParseCommitsTask))]
    [AfterTask(typeof(ParseWebLinksTask))]
    [BeforeTask(typeof(RenderTemplateTask))]
    internal sealed partial class ParseMarkdownWebLinksTask : SynchronousChangeLogTask
    {
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
                        MarkdownLinkParser.TryParseMarkdownLink(plainText.Text, out var linkText, out var linkDestination) &&
                        Uri.TryCreate(linkDestination, UriKind.Absolute, out var destination) &&
                        destination.IsWebLink())
                    {
                        m_Logger.LogDebug($"Detected Markdown link '{plainText.Text}' in footer for commit '{changeLogEntry.Commit.Id}'. Replacing footer value with web link");

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
