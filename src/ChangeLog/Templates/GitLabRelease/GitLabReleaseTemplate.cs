using System.Linq;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Templates.Default;
using Zio;
using Zio.FileSystems;

namespace Grynwald.ChangeLog.Templates.GitLabRelease
{
    /// <summary>
    /// Template optimized to produce a Markdown file for use as description text of a GitLab Release
    /// </summary>
    internal class GitLabReleaseTemplate : DefaultTemplate
    {
        /// <inheritdoc />
        protected override ChangeLogConfiguration.TemplateSettings TemplateSettings => m_Configuration.Template.GitLabRelease;

        /// <inheritdoc />
        public override TemplateName Name => TemplateName.GitLabRelease;


        public GitLabReleaseTemplate(ChangeLogConfiguration configuration) : base(configuration)
        { }


        /// <inheritdoc />
        public override void SaveChangeLog(ApplicationChangeLog changeLog, string outputPath)
        {
            if (changeLog.ChangeLogs.Count() > 1)
                throw new TemplateExecutionException("The GitLab Release template cannot render change logs that contain multiple versions");

            base.SaveChangeLog(changeLog, outputPath);
        }


        protected override IFileSystem GetTemplateFileSystem()
        {
            // GitLabRelease template is based on the "Default" template
            // => create aggregate file system with the files of both the "Default" and "GitLabRelease" templates
            var templateFileSystem = new AggregateFileSystem();
            templateFileSystem.AddFileSystem(base.GetTemplateFileSystem());
            templateFileSystem.AddFileSystem(CreateEmbeddedResourcesFileSystem("/templates/GitLabRelease"));

            return templateFileSystem;
        }
    }
}
