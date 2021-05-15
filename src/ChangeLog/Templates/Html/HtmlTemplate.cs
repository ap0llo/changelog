using System.Reflection;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.IO;
using Zio;

namespace Grynwald.ChangeLog.Templates.Html
{
    internal sealed class HtmlTemplate : ScribanBaseTemplate
    {
        /// <inheritdoc />
        protected override object TemplateSettings { get; }


        public HtmlTemplate(ChangeLogConfiguration configuration) : base(configuration)
        {
            TemplateSettings = configuration.Template.Html;
        }


        /// <inheritdoc />
        protected override ScribanTemplateLoader CreateTemplateLoader()
        {
            var embeddedResourcesFs = new EmbeddedResourcesFileSystem(Assembly.GetExecutingAssembly());
            var templateFileSystem = embeddedResourcesFs.GetOrCreateSubFileSystem("/templates/Html");

            return new FileSystemTemplateLoader(templateFileSystem, "/main.scriban-html");
        }
    }
}
