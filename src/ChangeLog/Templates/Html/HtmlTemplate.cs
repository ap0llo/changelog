using System.IO;
using System.Reflection;
using Grynwald.ChangeLog.Configuration;
using Scriban;

namespace Grynwald.ChangeLog.Templates.Html
{
    internal sealed class HtmlTemplate : ScribanBaseTemplate
    {
        private const string s_TemplateResourceName = "Grynwald.ChangeLog.Templates.Html.HtmlTemplate.html.scriban";


        public HtmlTemplate(ChangeLogConfiguration configuration) : base(configuration)
        { }


        protected override Template GetTemplate()
        {
            using var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(s_TemplateResourceName);
            using var streamReader = new StreamReader(resourceStream!);

            var templateString = streamReader.ReadToEnd();

            return Template.Parse(templateString);
        }
    }
}
