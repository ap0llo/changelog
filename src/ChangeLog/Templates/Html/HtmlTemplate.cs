using Grynwald.ChangeLog.Configuration;

namespace Grynwald.ChangeLog.Templates.Html
{
    internal sealed class HtmlTemplate : ScribanBaseTemplate
    {
        protected override object TemplateSettings { get; }


        public HtmlTemplate(ChangeLogConfiguration configuration) : base(configuration)
        {
            TemplateSettings = configuration.Template.Html;
        }


        protected override ScribanTemplateLoader CreateTemplateLoader() =>
            new EmbeddedResourceTemplateLoader(new[] { "templates/Html" }, "main.scriban-html");
    }
}
