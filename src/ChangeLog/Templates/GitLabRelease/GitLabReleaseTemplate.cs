using System;
using System.Linq;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.Model;
using Grynwald.MarkdownGenerator;

namespace Grynwald.ChangeLog.Templates.GitLabRelease
{
    /// <summary>
    /// Template optimized to produce a Markdown file for use as description text of a GitLab Release
    /// </summary>
    internal class GitLabReleaseTemplate : ITemplate
    {
        private readonly ChangeLogConfiguration m_Configuration;


        public GitLabReleaseTemplate(ChangeLogConfiguration configuration)
        {
            m_Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }


        public void SaveChangeLog(ApplicationChangeLog changeLog, string outputPath)
        {
            var document = new MdDocument();

            if (!changeLog.Versions.Any())
            {
                document.Root.Add(new MdParagraph(new MdEmphasisSpan("No changes found")));
            }

            document.Save(outputPath);
        }
    }
}
