using System;
using System.IO;

namespace docs.Validation
{
    public class TemplateOutputIsUpToDateRule : IRule
    {
        private const string s_RuleId = "TemplateOutputIsUpToDate";


        public void Apply(string path, ValidationResult result)
        {
            if (String.IsNullOrWhiteSpace(path))
                throw new ArgumentException($"'{nameof(path)}' cannot be null or whitespace", nameof(path));

            if (!IO.HasExtension(path, IO.FileExtensions.Scriban))
                return;

            if (IO.IsScribanPartial(path))
                return;

            var outputPath = IO.GetTemplateOutputPath(path);

            if (File.Exists(outputPath))
            {
                var expectedContent = DocsRenderer.RenderTemplate(path);
                var actualContent = File.ReadAllText(outputPath);

                if (!StringComparer.Ordinal.Equals(expectedContent, actualContent))
                {
                    result.AddError(s_RuleId, "Contents of output file are not up-to-date with respect to the template file");
                }
            }
            else
            {
                result.AddError(s_RuleId, $"Template output file does not exists (expected at '{outputPath}')");
            }
        }
    }
}
