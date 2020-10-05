using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LibGit2Sharp;
using Markdig;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Grynwald.ChangeLog.Test.DocsVerification
{
    /// <summary>
    /// Tests verifying Markdown documentation files.
    /// </summary>
    public class DocumentationTest : TestBase
    {
        private readonly ITestOutputHelper m_OutputHelper;


        public DocumentationTest(ITestOutputHelper outputHelper)
        {
            m_OutputHelper = outputHelper ?? throw new ArgumentNullException(nameof(outputHelper));
        }


        public static IEnumerable<object[]> MarkdownFiles()
        {
            foreach (var path in Directory.GetFiles(Path.Combine(RootPath, "docs"), "*.md", SearchOption.AllDirectories))
            {
                yield return new object[] { Path.GetRelativePath(RootPath, path) };
            }
            yield return new object[] { "README.md" };
        }

        [Theory]
        [MemberData(nameof(MarkdownFiles))]
        public void Documentation_links_are_valid(string rootRelativeInputPath)
        {
            var absoluteInputPath = Path.Combine(RootPath, rootRelativeInputPath);
            m_OutputHelper.WriteLine($"Verifying links in '{absoluteInputPath}'.");

            var parsed = ParseMarkdown(absoluteInputPath);

            var invalidLinks = parsed
                .Descendants<LinkInline>()
                .Where(link => !IsValidLink(absoluteInputPath, link.Url))
                .ToArray();

            if (invalidLinks.Any())
            {
                // Note: In Markdig, a file's first line is line 0, while most editors consider the first line to be line 1.
                // To make the output more understandable, adjust it for that difference and output line numbers as used by editors.
                throw new XunitException(
                    $"Markdown document '{rootRelativeInputPath}' contains invalid links:\r\n" +
                    String.Join("\r\n", invalidLinks.Select(x => $"  - '{x.Url}' (line {x.Line + 1})"))
                );
            }
        }


        private bool IsValidLink(string absoluteInputPath, string link)
        {
            m_OutputHelper.WriteLine($"Verifying link '{link}'.");

            // assume absolute links are always valid
            if (Uri.TryCreate(link, UriKind.Absolute, out _))
            {
                m_OutputHelper.WriteLine("Link is an absolute link, skipping check.");
                return true;
            }

            var (relativeTargetPath, targetAnchor) = ParseLink(link);
            m_OutputHelper.WriteLine($"Parsed link: target path '{relativeTargetPath}', target anchor '#{targetAnchor}'");

            if (String.IsNullOrEmpty(relativeTargetPath) && !String.IsNullOrEmpty(targetAnchor))
            {
                m_OutputHelper.WriteLine($"Checking same-page reference '#{targetAnchor}'");
                return IsValidAnchor(absoluteInputPath, targetAnchor);
            }
            else
            {
                var directoryPath = Path.GetDirectoryName(absoluteInputPath) ?? "";
                if (!Path.IsPathRooted(directoryPath))
                {
                    m_OutputHelper.WriteLine($"Failed to determine directory for input path '{absoluteInputPath}'");
                    return false;
                }

                var absoluteTargetPath = Path.GetFullPath(Path.Combine(directoryPath, relativeTargetPath));

                if (!File.Exists(absoluteTargetPath))
                {
                    m_OutputHelper.WriteLine($"Invalid link to '{relativeTargetPath}': Target '{absoluteTargetPath}' does not exist");
                    return false;
                }
                else if (!String.IsNullOrEmpty(targetAnchor) && Path.GetExtension(absoluteTargetPath).Equals(".md", StringComparison.OrdinalIgnoreCase))
                {
                    return IsValidAnchor(absoluteTargetPath, targetAnchor);
                }

            }

            m_OutputHelper.WriteLine($"Link verified successfully.");
            return true;
        }


        private bool IsValidAnchor(string filePath, string id)
        {
            m_OutputHelper.WriteLine($"Checking for id '{id}' in document '{filePath}'");

            var valid = GetHeadingIds(filePath).Contains(id);
            if (!valid)
            {
                m_OutputHelper.WriteLine($"Invalid link: id '{id}' not found in document '{filePath}'");
            }

            return valid;
        }

        private MarkdownDocument ParseMarkdown(string filePath)
        {
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            var parsed = Markdown.Parse(File.ReadAllText(filePath), pipeline);
            return parsed;
        }

        private static (string relativePath, string anchor) ParseLink(string link)
        {
            var index = link.IndexOf('#');
            if (index >= 0)
            {
                var anchor = link.Substring(index).TrimStart('#');
                var relativePath = link.Substring(0, index);

                return (relativePath, anchor);
            }
            else
            {
                return (link, "");
            }
        }

        private IReadOnlyCollection<string> GetHeadingIds(string path)
        {
            return ParseMarkdown(path)
                .Descendants<HeadingBlock>()
                .Select(x => x.GetAttributes().Id)
                .ToHashSet();
        }
    }
}
