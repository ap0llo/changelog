using System;
using Scriban;
using Scriban.Parsing;
using Zio;

namespace Grynwald.ChangeLog.Templates
{
    internal class FileSystemTemplateLoader : ScribanTemplateLoader
    {
        private readonly IFileSystem m_FileSystem;
        private readonly string m_EntryTemplatePath;


        public FileSystemTemplateLoader(IFileSystem fileSystem, string entryTemplatePath)
        {
            if (String.IsNullOrWhiteSpace(entryTemplatePath))
                throw new ArgumentException("Value must not be null or whitespace", nameof(entryTemplatePath));

            m_FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            m_EntryTemplatePath = entryTemplatePath;
        }


        public override Template LoadEntryTemplate()
        {
            var templateText = m_FileSystem.ReadAllText(m_EntryTemplatePath);
            return Template.Parse(templateText, m_EntryTemplatePath);
        }

        public override string GetPath(TemplateContext context, SourceSpan callerSpan, string templateRelativePath)
        {
            var path = new UPath(callerSpan.FileName).GetDirectory() / templateRelativePath;
            return path.ToAbsolute().FullName;
        }

        public override string Load(TemplateContext context, SourceSpan callerSpan, string templatePath) =>
            m_FileSystem.ReadAllText(templatePath);
    }
}
