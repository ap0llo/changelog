using System.Linq;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Templates.Default;
using Zio;
using Zio.FileSystems;

namespace Grynwald.ChangeLog.Templates.GitHubRelease
{
    internal class GitHubReleaseTemplate : DefaultTemplate
    {
        /// <inheritdoc />
        protected override ChangeLogConfiguration.TemplateSettings TemplateSettings => m_Configuration.Template.GitHubRelease;


        public GitHubReleaseTemplate(ChangeLogConfiguration configuration) : base(configuration)
        { }


        /// <inheritdoc />
        public override void SaveChangeLog(ApplicationChangeLog changeLog, string outputPath)
        {
            if (changeLog.ChangeLogs.Count() > 1)
                throw new TemplateExecutionException("The GitHub Release template cannot render change logs that contain multiple versions");

            base.SaveChangeLog(changeLog, outputPath);
        }

        protected override IFileSystem GetTemplateFileSystem()
        {
            // GitHubRelease template is based on the "Default" template
            // => create aggregate file system with the files of both the "Default" and "GitHubRelease" templates
            var templateFileSystem = new AggregateFileSystem();
            templateFileSystem.AddFileSystem(base.GetTemplateFileSystem());
            templateFileSystem.AddFileSystem(CreateEmbeddedResourcesFileSystem("/templates/GitHubRelease"));

            return templateFileSystem;
        }
    }
}
