using Grynwald.ChangeLog.Configuration;
using Zio;

namespace Grynwald.ChangeLog.Templates.Html
{
    internal sealed class HtmlTemplate : ScribanBaseTemplate
    {
        /// <inheritdoc />
        protected override ChangeLogConfiguration.TemplateSettings TemplateSettings => m_Configuration.Template.Html;

        /// <inheritdoc />
        protected override string TemplateFileExtension => ".scriban-html";


        public HtmlTemplate(ChangeLogConfiguration configuration) : base(configuration)
        { }


        /// <inheritdoc />
        protected override IFileSystem GetTemplateFileSystem() => CreateEmbeddedResourcesFileSystem("/templates/Html");
    }
}
