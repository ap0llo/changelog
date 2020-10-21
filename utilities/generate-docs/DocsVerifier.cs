using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace generate_docs
{
    internal static class DocsVerifier
    {
        public class VerificationResult
        {
            public bool Success => Errors.Any();

            public IReadOnlyList<string> Errors { get; }

            public VerificationResult(IReadOnlyList<string> errors)
            {
                Errors = errors ?? throw new ArgumentNullException(nameof(errors));
            }
        }

        public static VerificationResult VerifyDocument(string path)
        {
            var extension = Path.GetExtension(path);
            if (!StringComparer.OrdinalIgnoreCase.Equals(extension, ".md"))
                throw new InvalidOperationException($"Expected extension of document to verify to be '.md' but is '{extension}'");

            var errors = new List<string>();


            // If a scriban template exists for the file, verify the file is up-to-date
            var templatePath = $"{path}.scriban";
            if (File.Exists(templatePath))
            {
                var expectedContent = DocsRenderer.RenderTemplate(templatePath);
                var actualContent = File.ReadAllText(path);

                if (!StringComparer.Ordinal.Equals(expectedContent, actualContent))
                {
                    errors.Add("File contents are not up-to-date with regards to the scriban template");
                }
            }


            return new VerificationResult(errors);
        }

    }
}
