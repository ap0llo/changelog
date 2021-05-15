using System.Linq;
using System.Reflection;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.IO;
using Grynwald.ChangeLog.Model;
using Zio;
using Zio.FileSystems;

namespace Grynwald.ChangeLog.Templates.GitLabRelease
{
    /// <summary>
    /// Template optimized to produce a Markdown file for use as description text of a GitLab Release
    /// </summary>
    internal class GitLabReleaseTemplate : ScribanBaseTemplate
    {
        /// <inheritdoc />
        protected override object TemplateSettings { get; }


        public GitLabReleaseTemplate(ChangeLogConfiguration configuration) : base(configuration)
        {
            TemplateSettings = configuration.Template.GitLabRelease;
        }


        /// <inheritdoc />
        public override void SaveChangeLog(ApplicationChangeLog changeLog, string outputPath)
        {
            if (changeLog.ChangeLogs.Count() > 1)
                throw new TemplateExecutionException("The GitLab Release template cannot render change logs that contain multiple versions");

            base.SaveChangeLog(changeLog, outputPath);
        }


        /// <inheritdoc />
        protected override ScribanTemplateLoader CreateTemplateLoader()
        {
            var embeddedResourcesFs = new EmbeddedResourcesFileSystem(Assembly.GetExecutingAssembly());

            // GitLabRelease template is based on the "Default" template
            // => create aggregate file system with the files of both the "Default" and "GitLabRelease" templates
            var templateFileSystem = new AggregateFileSystem();
            templateFileSystem.AddFileSystem(embeddedResourcesFs.GetOrCreateSubFileSystem("/templates/Default"));
            templateFileSystem.AddFileSystem(embeddedResourcesFs.GetOrCreateSubFileSystem("/templates/GitLabRelease"));

            return new FileSystemTemplateLoader(templateFileSystem, "/main.scriban-txt");
        }

    }
}
