using Grynwald.ChangeLog.Configuration;
using Zio;

namespace Grynwald.ChangeLog.Templates.Default
{
    /// <summary>
    /// Implementation of the default template to convert a changelog to Markdown
    /// </summary>
    internal class DefaultTemplate : ScribanBaseTemplate
    {
        /// <inheritdoc />
        protected override ChangeLogConfiguration.TemplateSettings TemplateSettings => m_Configuration.Template.Default;

        /// <inheritdoc />
        protected override string TemplateFileExtension => ".scriban-txt";

        /// <inheritdoc />
        public override TemplateName Name => TemplateName.Default;


        public DefaultTemplate(ChangeLogConfiguration configuration) : base(configuration)
        { }


        /// <inheritdoc />
        protected override IFileSystem GetTemplateFileSystem() => CreateEmbeddedResourcesFileSystem("/templates/Default");
    }
}
