using System.Linq;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.Model;

namespace Grynwald.ChangeLog.Templates.GitHubRelease
{
    internal class GitHubReleaseTemplate : ScribanBaseTemplate
    {
        /// <inheritdoc />
        protected override object TemplateSettings { get; }


        public GitHubReleaseTemplate(ChangeLogConfiguration configuration) : base(configuration)
        {
            TemplateSettings = new
            {
                EnableNormalization = configuration.Template.GitHubRelease.NormalizeReferences
            };
        }


        /// <inheritdoc />
        public override void SaveChangeLog(ApplicationChangeLog changeLog, string outputPath)
        {
            if (changeLog.ChangeLogs.Count() > 1)
                throw new TemplateExecutionException("The GitHub Release template cannot render change logs that contain multiple versions");

            base.SaveChangeLog(changeLog, outputPath);
        }

        /// <inheritdoc />
        protected override ScribanTemplateLoader CreateTemplateLoader() =>
            new EmbeddedResourceTemplateLoader(new[] { "templates/GitHubRelease/", "templates/Default/" }, "main.scriban-txt");
    }
}
