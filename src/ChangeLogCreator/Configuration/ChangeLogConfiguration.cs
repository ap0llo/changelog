using System;

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
    }
}

