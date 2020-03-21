using System;
using System.IO;

namespace Grynwald.ChangeLog.Configuration
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


        public class FooterConfiguration
        {
            public string? Name { get; set; }

            public string? DisplayName { get; set; }
        }


        public enum IntegrationProvider
        {
            None = 0,
            GitHub = 1
        }

        public class GitHubIntegrationConfiguration
        {
            public string? AccessToken { get; set; } = null;
        }

        public class GitLabIntegrationConfiguration
        {
            public string? AccessToken { get; set; } = null;
        }

        public class IntegrationsConfiguration
        {
            public IntegrationProvider Provider { get; set; }

            public GitHubIntegrationConfiguration GitHub { get; set; } = new GitHubIntegrationConfiguration();

            public GitLabIntegrationConfiguration GitLab { get; set; } = new GitLabIntegrationConfiguration();
        }


        public ScopeConfiguration[] Scopes { get; set; } = Array.Empty<ScopeConfiguration>();

        public MarkdownConfiguration Markdown { get; set; } = new MarkdownConfiguration();

        public string[] TagPatterns { get; set; } = Array.Empty<string>();

        public string OutputPath { get; set; } = "";

        public string RepositoryPath { get; set; } = null!;

        public FooterConfiguration[] Footers { get; set; } = Array.Empty<FooterConfiguration>();

        public IntegrationsConfiguration Integrations { get; set; } = new IntegrationsConfiguration();

        public string? VersionRange { get; set; } = "";


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

