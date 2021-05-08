using Grynwald.ChangeLog.Configuration;

namespace Grynwald.ChangeLog.Templates.Html
{
    internal sealed class HtmlTemplate : ScribanBaseTemplate
    {
        protected override object TemplateSettings { get; }


        public HtmlTemplate(ChangeLogConfiguration configuration) : base(configuration)
        {
            TemplateSettings = new
            {
                EnableNormalization = configuration.Template.Html.NormalizeReferences
            };
        }


        protected override ScribanTemplateLoader CreateTemplateLoader() => new EmbeddedResourceTemplateLoader("templates/Html", "main.scriban-html");
    }
}
