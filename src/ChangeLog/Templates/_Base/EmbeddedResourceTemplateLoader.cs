using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Scriban;
using Scriban.Parsing;

namespace Grynwald.ChangeLog.Templates
{
    internal class EmbeddedResourceTemplateLoader : ScribanTemplateLoader
    {
        private const string s_Scheme = "embeddedResource";
        private static readonly HashSet<string> s_ResourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames().ToHashSet();

        private readonly IReadOnlyList<string> m_ResourceNamePrefixes;
        private readonly string m_EntryTemplateName;

        public EmbeddedResourceTemplateLoader(IEnumerable<string> resourceNamePrefixes, string entryTemplateName)
        {
            if (String.IsNullOrWhiteSpace(entryTemplateName))
                throw new ArgumentException("Value must not be null or whitespace", nameof(entryTemplateName));

            m_ResourceNamePrefixes = resourceNamePrefixes?.ToList() ?? throw new ArgumentNullException(nameof(resourceNamePrefixes));
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


        private string GetResourceUri(string templateName)
        {
            foreach (var prefix in m_ResourceNamePrefixes)
            {
                var resourceName = $"{prefix.TrimEnd('/')}/{templateName}";
                if (s_ResourceNames.Contains(resourceName))
                {
                    return $"{s_Scheme}:///{resourceName}";
                }
            }

            throw new TemplateExecutionException($"Failed to locate template '{templateName}'");
        }

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
