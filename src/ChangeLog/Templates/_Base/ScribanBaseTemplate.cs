using System;
using System.Collections;
using System.IO;
using System.Linq;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Model.Text;
using Grynwald.ChangeLog.Templates.ViewModel;
using Scriban;
using Scriban.Runtime;

namespace Grynwald.ChangeLog.Templates
{
    internal abstract class ScribanBaseTemplate : ITemplate
    {
        private class EnumerableFunctions : ScriptObject
        {
            public static bool Any(IEnumerable enumerable) => enumerable.Cast<object>().Any();

            public static int CountItems(IEnumerable enumerable) => enumerable.Cast<object>().Count();
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

            var templateContext = new TemplateContext();
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
    }
}
