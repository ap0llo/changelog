using System;
using System.IO;

namespace docs
{
    internal static class IO
    {
        public static class FileExtensions
        {
            public const string Markdown = ".md";
            public const string Scriban = ".scriban";
        }

        public static string GetTemplateOutputPath(string templatePath)
        {
            if (String.IsNullOrWhiteSpace(templatePath))
                throw new ArgumentException($"'{nameof(templatePath)}' cannot be null or whitespace", nameof(templatePath));

            if (!HasExtension(templatePath, FileExtensions.Scriban))
                throw new InvalidOperationException($"File '{templatePath}' is not a template");

            return Path.ChangeExtension(templatePath, "").TrimEnd('.');
        }

        public static string GetTemplatePath(string outputFilePath)
        {
            if (String.IsNullOrWhiteSpace(outputFilePath))
                throw new ArgumentException($"'{nameof(outputFilePath)}' cannot be null or whitespace", nameof(outputFilePath));

            return $"{outputFilePath}{FileExtensions.Scriban}";
        }

        public static bool HasExtension(string path, string extension)
        {
            if (String.IsNullOrEmpty(path))
                throw new ArgumentException($"'{nameof(path)}' cannot be null or empty", nameof(path));

            return StringComparer.OrdinalIgnoreCase.Equals(extension, Path.GetExtension(path));
        }
    }
}
