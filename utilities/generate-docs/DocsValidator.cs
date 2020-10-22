using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace generate_docs
{
    internal static class DocsValidator
    {
        public class ValidationResult
        {
            private readonly List<string> m_Errors = new List<string>();

            public bool Success => !Errors.Any();

            public IReadOnlyList<string> Errors => m_Errors;


            public void AddError(string error)
            {
                if (String.IsNullOrWhiteSpace(error))
                    throw new ArgumentException($"'{nameof(error)}' cannot be null or whitespace", nameof(error));

                m_Errors.Add(error);
            }
        }

        private interface IRule
        {
            void Apply(string path, ValidationResult result);
        }

        private class TemplateOutputIsUpToDateRule : IRule
        {
            public void Apply(string path, ValidationResult result)
            {
                if (String.IsNullOrWhiteSpace(path))
                    throw new ArgumentException($"'{nameof(path)}' cannot be null or whitespace", nameof(path));

                if (!IO.HasExtension(path, IO.FileExtensions.Scriban))
                    return;

                var outputPath = IO.GetTemplateOutputPath(path);

                if (File.Exists(outputPath))
                {
                    var expectedContent = DocsRenderer.RenderTemplate(path);
                    var actualContent = File.ReadAllText(outputPath);

                    if (!StringComparer.Ordinal.Equals(expectedContent, actualContent))
                    {
                        result.AddError("Contents of output file are not up-to-date with regards to the scriban template.");
                    }
                }
                else
                {
                    result.AddError($"Template output file does not exists (expected at '{outputPath}')");
                }
            }
        }


        private static readonly IReadOnlyList<IRule> s_Rules = new IRule[]
        {
            new TemplateOutputIsUpToDateRule()
        };


        public static ValidationResult ValidateDocument(string path)
        {
            if (String.IsNullOrWhiteSpace(path))
                throw new ArgumentException($"'{nameof(path)}' cannot be null or whitespace", nameof(path));

            var result = new ValidationResult();
            foreach (var rule in s_Rules)
            {
                rule.Apply(path, result);
            }

            return result;
        }

    }
}
