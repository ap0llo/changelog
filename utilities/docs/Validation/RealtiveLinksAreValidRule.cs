using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Markdig;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace docs.Validation
{
    internal class RealtiveLinksAreValidRule : IRule
    {
        private const string s_RuleId = "RealtiveLinksAreValid";


        public void Apply(string path, ValidationResult result)
        {
            if (String.IsNullOrWhiteSpace(path))
                throw new ArgumentException($"'{nameof(path)}' cannot be null or whitespace", nameof(path));

            if (!IO.HasExtension(path, IO.FileExtensions.Markdown))
                return;

            var parsed = ParseMarkdown(path);

            var absoluteInputPath = Path.GetFullPath(path);
            foreach (var link in parsed.Descendants<LinkInline>())
            {
                ValidateLink(absoluteInputPath, link, result);
            }
        }


        private void ValidateLink(string absoluteInputPath, LinkInline link, ValidationResult result)
        {
            if (!Path.IsPathRooted(absoluteInputPath))
                throw new ArgumentException("Input path must be rooted", nameof(absoluteInputPath));

            // assume absolute links are always valid
            if (Uri.TryCreate(link.Url, UriKind.Absolute, out _))
                return;

            // In Markdig, a file's first line is line 0, while most editors consider the first line to be line 1.
            // To make the output more understandable, adjust it for that difference and output line numbers as used by editors.
            var lineNumber = link.Line + 1;

            if (link.Url is null)
                return;

            var (relativeTargetPath, targetAnchor) = ParseLink(link.Url);

            // check same-page link
            if (String.IsNullOrEmpty(relativeTargetPath) && !String.IsNullOrEmpty(targetAnchor))
            {
                if (!IsValidAnchor(absoluteInputPath, targetAnchor))
                    result.AddError(s_RuleId, $"Invalid link '{link.Url}': Anchor '{targetAnchor}' does not exist in {absoluteInputPath}", lineNumber);
            }
            else
            {
                var directoryPath = Path.GetDirectoryName(absoluteInputPath) ?? "";
                var absoluteTargetPath = Path.GetFullPath(Path.Combine(directoryPath, relativeTargetPath));

                if (!File.Exists(absoluteTargetPath))
                {
                    result.AddError(s_RuleId, $"Invalid link '{link.Url}': Target path '{absoluteTargetPath}' does not exist", lineNumber);
                }
                else if (!String.IsNullOrEmpty(targetAnchor) && Path.GetExtension(absoluteTargetPath).Equals(".md", StringComparison.OrdinalIgnoreCase))
                {
                    if (!IsValidAnchor(absoluteTargetPath, targetAnchor))
                        result.AddError(s_RuleId, $"Invalid link '{link.Url}': Anchor '{targetAnchor}' does not exist in {absoluteTargetPath}", lineNumber);
                }
            }
        }

        private bool IsValidAnchor(string filePath, string id) => GetHeadingIds(filePath).Contains(id);

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
                .Where(x => x is not null)
                .Select(x => x!)
                .ToHashSet();
        }
    }
}
