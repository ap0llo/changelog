using System;
using System.IO;

namespace Grynwald.ChangeLog.Configuration
{
    public class ChangeLogConfiguration
    {
        public class ScopeConfiguration
        {
            public string Name { get; set; } = "";

            public string? DisplayName { get; set; }
        }

        public class FooterConfiguration
        {
            public string? Name { get; set; }

            public string? DisplayName { get; set; }
        }

        public enum IntegrationProvider
        {
            None = 0,
            GitHub = 1,
            GitLab = 2,
        }

        public class GitHubIntegrationConfiguration
        {
            public string? AccessToken { get; set; } = null;

            public string? Host { get; set; } = null;

            public string? Owner { get; set; } = null;

            public string? Repository { get; set; } = null;
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

        public enum TemplateName
        {
            Default,
            GitLabRelease,
            GitHubRelease
        }

        public class TemplateConfiguration
        {
            public TemplateName Name { get; set; }

            public DefaultTemplateConfiguration Default { get; set; } = new DefaultTemplateConfiguration();
        }

        public enum MarkdownPreset
        {
            Default,
            MkDocs
        }

        public class DefaultTemplateConfiguration
        {
            public MarkdownPreset MarkdownPreset { get; set; } = MarkdownPreset.Default;
        }

        public class EntryTypeConfiguration
        {
            public string Type { get; set; } = "";

            public string? DisplayName { get; set; }
        }

        public enum ParserMode
        {
            Strict,
            Loose
        }

        public class ParserConfiguration
        {
            public ParserMode Mode { get; set; }
        }


        public ScopeConfiguration[] Scopes { get; set; } = Array.Empty<ScopeConfiguration>();

        public string[] TagPatterns { get; set; } = Array.Empty<string>();

        public string OutputPath { get; set; } = "";

        public string RepositoryPath { get; set; } = null!;

        public FooterConfiguration[] Footers { get; set; } = Array.Empty<FooterConfiguration>();

        public IntegrationsConfiguration Integrations { get; set; } = new IntegrationsConfiguration();

        public string? VersionRange { get; set; } = "";

        public string? CurrentVersion { get; set; }

        public TemplateConfiguration Template { get; set; } = new TemplateConfiguration();

        public EntryTypeConfiguration[] EntryTypes { get; set; } = Array.Empty<EntryTypeConfiguration>();

        public ParserConfiguration Parser { get; set; } = new ParserConfiguration();


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

