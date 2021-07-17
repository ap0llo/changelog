using System;
using System.Linq;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Model.Text;
using Grynwald.ChangeLog.Pipeline;

namespace Grynwald.ChangeLog.Tasks
{
    /// <summary>
    /// Detects footer values that are web links.
    /// When a valid url is found, replaces  the footer's value (see <see cref="ChangeLogEntryFooter.Value"/>) with a <see cref="WebLinkTextElement"/>.
    /// </summary>
    [AfterTask(typeof(ParseCommitsTask))]
    [BeforeTask(typeof(RenderTemplateTask))]
    internal sealed class ParseWebLinksTask : SynchronousChangeLogTask
    {
        protected override ChangeLogTaskResult Run(ApplicationChangeLog changelog)
        {
            var allFooters = changelog.ChangeLogs.SelectMany(x => x.AllEntries).SelectMany(x => x.Footers);
            foreach (var footer in allFooters)
            {
                if (footer.Value is PlainTextElement plainText &&
                   Uri.TryCreate(plainText.Text, UriKind.Absolute, out var uri) &&
                   IsWebLink(uri))
                {
                    footer.Value = new WebLinkTextElement(plainText.Text, uri);
                }
            }

            return ChangeLogTaskResult.Success;
        }

        private bool IsWebLink(Uri uri) => uri.Scheme.ToLower() switch
        {
            "http" => true,
            "https" => true,
            _ => false
        };
    }
}
