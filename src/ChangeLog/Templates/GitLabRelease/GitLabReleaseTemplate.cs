using System.Linq;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.Model;

namespace Grynwald.ChangeLog.Templates.GitLabRelease
{
    /// <summary>
    /// Template optimized to produce a Markdown file for use as description text of a GitLab Release
    /// </summary>
    internal class GitLabReleaseTemplate : ScribanBaseTemplate
    {
        protected override object TemplateSettings { get; }


        public GitLabReleaseTemplate(ChangeLogConfiguration configuration) : base(configuration)
        {
            TemplateSettings = configuration.Template.GitLabRelease;
        }


        public override void SaveChangeLog(ApplicationChangeLog changeLog, string outputPath)
        {
            if (changeLog.ChangeLogs.Count() > 1)
                throw new TemplateExecutionException("The GitLab Release template cannot render change logs that contain multiple versions");

            base.SaveChangeLog(changeLog, outputPath);
        }


        protected override ScribanTemplateLoader CreateTemplateLoader() =>
            new EmbeddedResourceTemplateLoader(new[] { "templates/GitLabRelease/", "templates/Default/" }, "main.scriban-txt");
    }
}
