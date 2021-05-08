using Grynwald.ChangeLog.Configuration;

namespace Grynwald.ChangeLog.Templates.Markdown
{
    internal class MarkdownTemplate : ScribanBaseTemplate
    {
        protected override object TemplateSettings { get; }


        public MarkdownTemplate(ChangeLogConfiguration configuration) : base(configuration)
        {
            TemplateSettings = new
            {
                EnableNormalization = configuration.Template.Markdown.NormalizeReferences
            };
        }

        protected override ScribanTemplateLoader CreateTemplateLoader() =>
            new EmbeddedResourceTemplateLoader("templates/Markdown/", "main.scriban-txt");
    }
}
