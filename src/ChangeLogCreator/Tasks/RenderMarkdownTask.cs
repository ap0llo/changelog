using System;
using System.Linq;
using ChangeLogCreator.Model;
using Grynwald.MarkdownGenerator;

namespace ChangeLogCreator.Tasks
{
    internal sealed class RenderMarkdownTask : IChangeLogTask
    {
        private readonly string m_OutputPath;


        public MdSerializationOptions SerializationOptions { get; }


        public RenderMarkdownTask(string outputPath)
        {
            m_OutputPath = outputPath ?? throw new ArgumentNullException(nameof(outputPath));

            //TODO: Preset needs to be configurable
            SerializationOptions = MdSerializationOptions.Presets.MkDocs.With(opts => { opts.HeadingAnchorStyle = MdHeadingAnchorStyle.Tag; });
        }


        public void Run(ChangeLog changeLog)
        {
            var document = GetChangeLogDocument(changeLog);
            document.Save(m_OutputPath, SerializationOptions); 
        }


        internal MdDocument GetChangeLogDocument(ChangeLog changeLog)
        {
            var document = new MdDocument();
            document.Root.Add(new MdHeading(1, "Change Log") { Anchor = "changelog-heading-root" } );

            var first = true;
            foreach (var versionChangeLog in changeLog.ChangeLogs)
            {
                if (!first)
                    document.Root.Add(new MdThematicBreak());

                document.Root.Add(GetVersionSection(versionChangeLog));
                first = false;
            }

            return document;
        }


        private MdBlock GetVersionSection(SingleVersionChangeLog versionChangeLog)
        {
            //TODO: Different layout for versions with a single change

            var block = new MdContainerBlock
            {
                new MdHeading(2, versionChangeLog.Version.Version.ToNormalizedString()) { Anchor = GetHtmlHeadingId(versionChangeLog) }
            };

            var isEmpty = true;

            var features = versionChangeLog.FeatureEntries.ToArray();
            if (features.Length > 0)
            {
                isEmpty = false;
                block.Add(new MdHeading(3, "New Features") { Anchor = GetHtmlHeadingId(versionChangeLog, "features") } );
                block.Add(new MdBulletList(features.Select(ToListItem)));
            }

            var bugFixes = versionChangeLog.BugFixEntries.ToArray();
            if (bugFixes.Length > 0)
            {
                isEmpty = false;
                block.Add(new MdHeading(3, "Bug Fixes") { Anchor = GetHtmlHeadingId(versionChangeLog, "bugfixes") });
                block.Add(new MdBulletList(bugFixes.Select(ToListItem)));
            }

            if(!isEmpty)
            {
                //TODO: Option to omit details section

                block.Add(new MdHeading(3, "Details") { Anchor = GetHtmlHeadingId(versionChangeLog, "details") });

                foreach(var feature in features)
                {
                    block.Add(GetDetailsBlock(feature));
                }
                foreach (var bugFix in bugFixes)
                {
                    block.Add(GetDetailsBlock(bugFix));
                }
            }

            if (isEmpty)
            {
                block.Add(new MdParagraph(new MdEmphasisSpan("No changes found.")));
            }

            //TODO: Special handling for breaking changes

            return block;
        }

        private MdListItem ToListItem(ChangeLogEntry entry)
        {
            var text = GetSummaryText(entry);
            var id = GetHtmlHeadingId(entry);

            return new MdListItem(new MdParagraph(
                new MdLinkSpan(text, $"#{id}")
            ));
        }

        private static MdSpan GetSummaryText(ChangeLogEntry entry)
        {
            return entry.Scope switch
            {
                string s when !String.IsNullOrEmpty(s) =>
                    new MdCompositeSpan(
                        new MdStrongEmphasisSpan($"{entry.Scope}:"),
                        new MdTextSpan($" {entry.Summary}")),

                _ => new MdTextSpan(entry.Summary),
            };
        }

        private MdBlock GetDetailsBlock(ChangeLogEntry entry)
        {
            var block = new MdContainerBlock
            {
                new MdHeading(4, GetSummaryText(entry)) { Anchor = GetHtmlHeadingId(entry) }
            };

            foreach (var paragraph in entry.Body)
            {
                block.Add(new MdParagraph(paragraph));
            }

            block.Add(
                new MdBulletList(
                    new MdListItem("Commit: ", new MdCodeSpan(entry.Commit.Id))
            ));

            return block;
        }

        private string GetHtmlHeadingId(ChangeLogEntry entry)
        {
            return $"changelog-heading-{entry.Commit}";
        }

        private string GetHtmlHeadingId(SingleVersionChangeLog changelog, string? suffix = null)
        {
            var id = $"changelog-heading-{HtmlUtilities.ToUrlSlug(changelog.Version.Version.ToNormalizedString())}";
            if(suffix != null)
            {
                id += "-" + suffix;
            }

            return id;
        }

    }
}
