using System.Reflection;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.IO;
using Zio;

namespace Grynwald.ChangeLog.Templates.Default
{
    /// <summary>
    /// Implementation of the default template to convert a changelog to Markdown
    /// </summary>
    internal class DefaultTemplate : ScribanBaseTemplate
    {
        /// <inheritdoc />
        protected override object TemplateSettings { get; }


        public DefaultTemplate(ChangeLogConfiguration configuration) : base(configuration)
        {
            TemplateSettings = configuration.Template.Default;
        }

        /// <inheritdoc />
        protected override ScribanTemplateLoader CreateTemplateLoader()
        {
            var embeddedResourcesFs = new EmbeddedResourcesFileSystem(Assembly.GetExecutingAssembly());
            var templateFileSystem = embeddedResourcesFs.GetOrCreateSubFileSystem("/templates/Default");

            return new FileSystemTemplateLoader(templateFileSystem, "/main.scriban-txt");
        }
    }
}
