using Grynwald.ChangeLog.Configuration;

namespace Grynwald.ChangeLog.Templates.Default
{
    /// <summary>
    /// Implementation of the default template to convert a changelog to Markdown
    /// </summary>
    internal class DefaultTemplate : ScribanBaseTemplate
    {
        protected override object TemplateSettings { get; }

        public DefaultTemplate(ChangeLogConfiguration configuration) : base(configuration)
        {
            TemplateSettings = new
            {
                EnableNormalization = configuration.Template.Default.NormalizeReferences
            };
        }

        protected override ScribanTemplateLoader CreateTemplateLoader() =>
            new EmbeddedResourceTemplateLoader("templates/Default/", "main.scriban-txt");
    }
}
