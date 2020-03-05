using System;
using System.Linq;
using ChangeLogCreator.Model;
using Grynwald.MarkdownGenerator;

namespace ChangeLogCreator.Tasks
{
    internal sealed class RenderMarkdownTask : IChangeLogTask
    {
        private readonly string m_OutputPath;


        public RenderMarkdownTask(string outputPath)
        {
            m_OutputPath = outputPath ?? throw new ArgumentNullException(nameof(outputPath));
        }


        public void Run(ChangeLog changeLog)
        {
            var document = GetChangeLogDocument(changeLog);
            document.Save(m_OutputPath);
        }


        internal MdDocument GetChangeLogDocument(ChangeLog changeLog)
        {
            var document = new MdDocument();
            document.Root.Add(new MdHeading(1, "Change Log"));

            foreach (var versionChangeLog in changeLog.ChangeLogs)
            {
                document.Root.Add(GetVersionSection(versionChangeLog));
            }

            return document;
        }


        private MdBlock GetVersionSection(SingleVersionChangeLog versionChangeLog)
        {
            var block = new MdContainerBlock
            {
                new MdHeading(2, versionChangeLog.Version.Version.ToNormalizedString())
            };

            var features = versionChangeLog.FeatureEntries.ToArray();
            var bugFixes = versionChangeLog.BugFixEntries.ToArray();

            if (features.Length > 0)
            {
                block.Add(new MdHeading(3, "New Features"));
                block.Add(new MdBulletList(features.Select(ToListItem)));
            }

            if (bugFixes.Length > 0)
            {
                block.Add(new MdHeading(3, "Bug Fixes"));
                block.Add(new MdBulletList(bugFixes.Select(ToListItem)));
            }

            if (features.Length == 0 && bugFixes.Length == 0)
            {
                block.Add(new MdParagraph(new MdEmphasisSpan("No changes found.")));
            }


            //TODO: Special handling for breaking changes

            return block;
        }

        private MdListItem ToListItem(ChangeLogEntry entry)
        {
            MdSpan text = entry.Scope switch
            {
                string s when !String.IsNullOrEmpty(s) =>
                    new MdCompositeSpan(
                        new MdStrongEmphasisSpan($"{entry.Scope}:"),
                        new MdTextSpan($" {entry.Summary} ({entry.Commit})")),

                _ => new MdTextSpan($"{entry.Summary} ({entry.Commit})"),
            };

            return new MdListItem(new MdParagraph(text));
        }
    }
}
