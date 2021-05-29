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

        internal static IFileSystem GetTemplateFileSystem()
        {
            var embeddedResourcesFs = new EmbeddedResourcesFileSystem(Assembly.GetExecutingAssembly());
            var templateFileSystem = embeddedResourcesFs.GetOrCreateSubFileSystem("/templates/Html");

            return templateFileSystem;
        }


        /// <inheritdoc />
        protected override ScribanTemplateLoader CreateTemplateLoader()
        {
            return new FileSystemTemplateLoader(GetTemplateFileSystem(), "/main.scriban-html");
        }


    }
}
