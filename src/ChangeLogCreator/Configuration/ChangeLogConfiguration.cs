using System;
using System.IO;

namespace ChangeLogCreator.Configuration
{
    public class ChangeLogConfiguration
    {
        public class ScopeConfiguration
        {
            public string? Name { get; set; }

            public string? DisplayName { get; set; }
        }

        public enum MarkdownPreset
        {
            Default,
            MkDocs
        }

        public class MarkdownConfiguration
        {
            public MarkdownPreset Preset { get; set; } = MarkdownPreset.Default;
        }


        public ScopeConfiguration[] Scopes { get; set; } = Array.Empty<ScopeConfiguration>();

        public MarkdownConfiguration Markdown { get; set; } = new MarkdownConfiguration();

        public string[] TagPatterns { get; set; } = Array.Empty<string>();

        public string OutputPath { get; set; } = "";

        public string RepositoryPath { get; set; } = null!;


        public string GetFullOutputPath()
        {
            if (Path.IsPathRooted(OutputPath))
            {
                return Path.GetFullPath(OutputPath);
            }
            else
            {
                var path = Path.Combine(RepositoryPath, OutputPath);
                return Path.GetFullPath(path);
            }
        }
    }
}

