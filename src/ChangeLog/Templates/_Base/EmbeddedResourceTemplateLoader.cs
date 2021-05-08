using System;
using System.Reflection;
using Scriban;
using Scriban.Parsing;

namespace Grynwald.ChangeLog.Templates
{
    internal class EmbeddedResourceTemplateLoader : ScribanTemplateLoader
    {
        private const string s_Scheme = "embeddedResource";
        private readonly string m_Prefix;
        private readonly string m_EntryTemplateName;

        public EmbeddedResourceTemplateLoader(string prefix, string entryTemplateName)
        {
            if (String.IsNullOrWhiteSpace(prefix))
                throw new ArgumentException("Value must not be null or whitespace", nameof(prefix));

            if (String.IsNullOrWhiteSpace(entryTemplateName))
                throw new ArgumentException("Value must not be null or whitespace", nameof(entryTemplateName));

            m_Prefix = prefix;
            m_EntryTemplateName = entryTemplateName;
        }


        public override string LoadEntryTemplate()
        {
            var entryTemplateUri = GetResourceUri(m_EntryTemplateName);
            return LoadEmbeddedResource(entryTemplateUri);
        }

        public override string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName) =>
            GetResourceUri(templateName);

        public override string Load(TemplateContext context, SourceSpan callerSpan, string templatePath) =>
            LoadEmbeddedResource(templatePath);


        private string GetResourceUri(string templateName) => $"{s_Scheme}:///{m_Prefix.TrimEnd('/')}/{templateName}";

        private string LoadEmbeddedResource(string resourceUri)
        {
            var uri = new Uri(resourceUri, UriKind.Absolute);

            if (!StringComparer.OrdinalIgnoreCase.Equals(uri.Scheme, s_Scheme))
                throw new ArgumentException($"Unsupported scheme '{uri.Scheme}'", nameof(resourceUri));

            var resourceName = uri.PathAndQuery.TrimStart('/');
            return Assembly.GetExecutingAssembly().ReadEmbeddedResource(resourceName);
        }
    }
}
