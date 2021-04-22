using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Model.Text;
using Grynwald.ChangeLog.Templates.ViewModel;
using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;

namespace Grynwald.ChangeLog.Templates
{
    internal abstract class ScribanBaseTemplate : ITemplate, ITemplateLoader
    {
        private class EnumerableFunctions : ScriptObject
        {
            public static bool Any(IEnumerable enumerable) => enumerable.Cast<object>().Any();

            public new static int Count(IEnumerable enumerable) => enumerable.Cast<object>().Count();
        }

        private class TextElementFunctions : ScriptObject
        {
            public static bool IsLink(ITextElement element) => element is IWebLinkTextElement;

            public static bool IsChangeLogEntryReference(ITextElement element) => element is ChangeLogEntryReferenceTextElement;
        }


        private readonly ChangeLogConfiguration m_Configuration;


        public ScribanBaseTemplate(ChangeLogConfiguration configuration)
        {
            m_Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }


        public void SaveChangeLog(ApplicationChangeLog changeLog, string outputPath)
        {
            var viewModel = new ApplicationChangeLogViewModel(m_Configuration, changeLog);

            var template = GetTemplate();

            var templateContext = new TemplateContext()
            {
                TemplateLoader = this
            };
            var rootScriptObject = new ScriptObject()
            {
                { "model", viewModel },
                { "enumerable", new EnumerableFunctions() },
                { "textelement", new TextElementFunctions() },
            };
            templateContext.PushGlobal(rootScriptObject);

            

            var rendered = template.Render(templateContext);
            File.WriteAllText(outputPath, rendered);
        }


        protected abstract Template GetTemplate();

        public string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName) => templateName;

        public abstract string Load(TemplateContext context, SourceSpan callerSpan, string templatePath);

        public ValueTask<string> LoadAsync(TemplateContext context, SourceSpan callerSpan, string templatePath)
        {
            var template = Load(context, callerSpan, templatePath);
            return new ValueTask<string>(template);
        }
    }
}
