using System.Threading.Tasks;
using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;

namespace Grynwald.ChangeLog.Templates
{
    internal abstract class ScribanTemplateLoader : ITemplateLoader
    {
        public abstract string LoadEntryTemplate();

        public abstract string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName);

        public abstract string Load(TemplateContext context, SourceSpan callerSpan, string templatePath);

        public ValueTask<string> LoadAsync(TemplateContext context, SourceSpan callerSpan, string templatePath)
        {
            var template = Load(context, callerSpan, templatePath);
            return new ValueTask<string>(template);
        }
    }
}
