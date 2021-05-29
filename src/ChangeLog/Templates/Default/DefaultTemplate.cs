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


        internal static IFileSystem GetTemplateFileSystem()
        {
            var embeddedResourcesFs = new EmbeddedResourcesFileSystem(Assembly.GetExecutingAssembly());
            return embeddedResourcesFs.GetOrCreateSubFileSystem("/templates/Default");
        }


        /// <inheritdoc />
        protected override ScribanTemplateLoader CreateTemplateLoader()
        {
            return new FileSystemTemplateLoader(GetTemplateFileSystem(), "/main.scriban-txt");
        }
    }
}
