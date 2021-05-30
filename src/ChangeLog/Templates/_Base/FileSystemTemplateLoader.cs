using System;
using Scriban;
using Scriban.Parsing;
using Zio;

namespace Grynwald.ChangeLog.Templates
{
    internal class FileSystemTemplateLoader : ScribanTemplateLoader
    {
        private readonly IFileSystem m_FileSystem;


        public FileSystemTemplateLoader(IFileSystem fileSystem)
        {
            m_FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        }


        public override Template Load(string templatePath)
        {
            var templateText = m_FileSystem.ReadAllText(templatePath);
            return Template.Parse(templateText, templatePath);
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
