﻿using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.Git;
using Grynwald.ChangeLog.IO;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Model.Text;
using Grynwald.ChangeLog.Templates.ViewModel;
using Scriban;
using Scriban.Runtime;
using Scriban.Syntax;
using Zio;
using Zio.FileSystems;

namespace Grynwald.ChangeLog.Templates
{
    internal abstract class ScribanBaseTemplate : ITemplate
    {
        private class EnumerableFunctions : ScriptObject
        {
            public static bool Any(IEnumerable enumerable) => enumerable.Cast<object>().Any();

            public static new int Count(IEnumerable enumerable) => enumerable.Cast<object>().Count();

            public static object First(IEnumerable enumerable) => enumerable.Cast<object>().First();

            public static object Single(IEnumerable enumerable) => enumerable.Cast<object>().Single();
        }

        private class TextElementFunctions : ScriptObject
        {
            public static bool IsNormalizable(ITextElement element) => element is INormalizedTextElement;

            public static bool IsLink(ITextElement element) => element is IWebLinkTextElement;

            public static bool IsChangeLogEntryReference(ITextElement element) => element is ChangeLogEntryReferenceTextElement;
        }

        private class ChangeLogFunctions : ScriptObject
        {
            public static ChangeLogEntryViewModel? FindEntry(ApplicationChangeLogViewModel model, GitId commit)
            {
                return model.ChangeLogs.SelectMany(x => x.AllEntries).SingleOrDefault(x => x.Commit == commit);
            }
        }

        private class HtmlUtilities : ScriptObject
        {
            /// <summary>
            /// Generates a "url slug" from the specified value that can be used as a HTML id.
            /// </summary>
            /// <remarks>
            /// This method can be used to generate a id for linking within a document (typically a link to a heading).
            /// <para>
            /// There is no official spec for how anchors for headings in Markdown work.
            /// This implementation follows the guidance from <see href="https://stackoverflow.com/questions/27981247/github-markdown-same-page-link">Stack Overflow</see>:
            /// <list type="bullet">
            ///     <item><description>Leading and trailing whitespace is dropped.</description></item>
            ///     <item><description>Punctuation marks are dropped.</description></item>
            ///     <item><description>Upper case letters are converted to lower case.</description></item>
            ///     <item><description>Spaces are replaced with <c>-</c></description></item>
            /// </list>
            /// </para>
            /// </remarks>
            public static string Slugify(string? value)
            {
                value = value?.Trim();

                if (String.IsNullOrEmpty(value))
                {
                    return "";
                }

                var slug = new StringBuilder();

                foreach (var c in value!)
                {
                    if (Char.IsLetter(c) || Char.IsNumber(c))
                    {
                        slug.Append(Char.ToLower(c));
                    }
                    else if (Char.IsWhiteSpace(c))
                    {
                        slug.Append('-');
                    }
                }

                return slug.ToString();
            }
        }


        private readonly Lazy<IFileSystem> m_Filesystem;
        protected readonly ChangeLogConfiguration m_Configuration;


        protected abstract ChangeLogConfiguration.TemplateSettings TemplateSettings { get; }

        protected abstract string TemplateFileExtension { get; }

        public IFileSystem FileSystem => m_Filesystem.Value;

        /// <inheritdoc />
        public abstract TemplateName Name { get; }


        public ScribanBaseTemplate(ChangeLogConfiguration configuration)
        {
            m_Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            m_Filesystem = new Lazy<IFileSystem>(() =>
            {
                var fileSystem = GetTemplateFileSystem();

                if (!String.IsNullOrEmpty(TemplateSettings.CustomDirectory))
                {
                    var physicalFileSystem = new PhysicalFileSystem();
                    var path = physicalFileSystem.ConvertPathFromInternal(TemplateSettings.CustomDirectory);

                    var aggregateFileSystem = new AggregateFileSystem(fileSystem);
                    aggregateFileSystem.AddFileSystem(physicalFileSystem.GetOrCreateSubFileSystem(path));
                    fileSystem = aggregateFileSystem;
                }

                return fileSystem;
            });
        }


        /// <inheritdoc />
        public virtual void SaveChangeLog(ApplicationChangeLog changeLog, string outputPath)
        {
            var viewModel = new ApplicationChangeLogViewModel(m_Configuration, changeLog);

            var templateLoader = new FileSystemTemplateLoader(FileSystem);

            var templateContext = new TemplateContext()
            {
                TemplateLoader = templateLoader
            };
            var rootScriptObject = new ScriptObject()
            {
                { "model", viewModel },
                { "enumerable", new EnumerableFunctions() },
                { "textelement", new TextElementFunctions() },
                { "html_utilities", new HtmlUtilities() },
                { "changelog", new ChangeLogFunctions() },
                { "template_settings", TemplateSettings }
            };
            templateContext.PushGlobal(rootScriptObject);

            try
            {
                var template = templateLoader.Load($"/main{TemplateFileExtension}");
                var rendered = template.Render(templateContext);
                File.WriteAllText(outputPath, rendered);
            }
            catch (ScriptRuntimeException ex)
            {
                throw new TemplateExecutionException(ex.Message, ex);
            }
        }


        protected abstract IFileSystem GetTemplateFileSystem();

        protected static IFileSystem CreateEmbeddedResourcesFileSystem(string subFolder = "")
        {
            IFileSystem fileSystem = new EmbeddedResourcesFileSystem(Assembly.GetExecutingAssembly());
            if (!String.IsNullOrEmpty(subFolder))
            {
                fileSystem = fileSystem.GetOrCreateSubFileSystem(subFolder);
            }
            return fileSystem;
        }
    }
}
