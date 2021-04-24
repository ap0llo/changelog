using Grynwald.ChangeLog.Configuration;

namespace Grynwald.ChangeLog.Templates.Html
{
    internal sealed class HtmlTemplate : ScribanBaseTemplate
    {
        public HtmlTemplate(ChangeLogConfiguration configuration) : base(configuration)
        { }


        protected override ScribanTemplateLoader CreateTemplateLoader() => new EmbeddedResourceTemplateLoader("templates/Html", "main.html.scriban");
    }
}
