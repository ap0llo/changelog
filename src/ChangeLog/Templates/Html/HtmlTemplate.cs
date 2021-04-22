using System.IO;
using System.Reflection;
using Grynwald.ChangeLog.Configuration;
using Scriban;
using Scriban.Parsing;

namespace Grynwald.ChangeLog.Templates.Html
{
    internal sealed class HtmlTemplate : ScribanBaseTemplate
    {        
        private const string s_TemplateResourceName = "Grynwald.ChangeLog.Templates.Html.HtmlTemplate.html.scriban";


        public HtmlTemplate(ChangeLogConfiguration configuration) : base(configuration)
        { }

        public override string Load(TemplateContext context, SourceSpan callerSpan, string templatePath)
        {
            return LoadEmbeddedResource($"Grynwald.ChangeLog.Templates.Html.partial_{templatePath}.html.scriban");
        }

        protected override Template GetTemplate()
        {            
            var templateString = LoadEmbeddedResource(s_TemplateResourceName);
            return Template.Parse(templateString);
        }

        private string LoadEmbeddedResource(string resourceName)
        {
            using var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
            using var streamReader = new StreamReader(resourceStream!);

            return streamReader.ReadToEnd();
        }
    }
}
