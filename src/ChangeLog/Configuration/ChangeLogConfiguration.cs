using System;
using System.Collections.Generic;
using System.IO;
using Grynwald.ChangeLog.Validation;

namespace Grynwald.ChangeLog.Configuration
{
    public class ChangeLogConfiguration
    {
        public class ScopeConfiguration
        {
            public string? DisplayName { get; set; }
        }

        public class FooterConfiguration
        {
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
            [ValidationDisplayName("GitHub Access Token")]
            public string? AccessToken { get; set; } = null;

            [ValidationDisplayName("GitHub Remote Name")]
            public string? RemoteName { get; set; } = null;

            [ValidationDisplayName("GitHub Host")]
            public string? Host { get; set; } = null;

            [ValidationDisplayName("GitHub Owner Name")]
            public string? Owner { get; set; } = null;

            [ValidationDisplayName("GitHub Repository Name")]
            public string? Repository { get; set; } = null;
        }

        public class GitLabIntegrationConfiguration
        {
            [ValidationDisplayName("GitLab Access Token")]
            public string? AccessToken { get; set; } = null;

            [ValidationDisplayName("GitLab Remote Name")]
            public string? RemoteName { get; set; } = null;

            [ValidationDisplayName("GitLab Host")]
            public string? Host { get; set; } = null;

            [ValidationDisplayName("GitLab Namespace")]
            public string? Namespace { get; set; } = null;

            [ValidationDisplayName("GitLab Project Name")]
            public string? Project { get; set; } = null;
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
            [ValidationDisplayName("Entry Type")]
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


        public class FilterConfiguration
        {
            public FilterExpressionConfiguration[] Include { get; set; } = Array.Empty<FilterExpressionConfiguration>();

            public FilterExpressionConfiguration[] Exclude { get; set; } = Array.Empty<FilterExpressionConfiguration>();
        }

        public class FilterExpressionConfiguration
        {
            [ValidationDisplayName("Filter Type Expression")]
            public string Type { get; set; } = "*";

            [ValidationDisplayName("Filter Scope Expression")]
            public string Scope { get; set; } = "*";
        }


        public Dictionary<string, ScopeConfiguration> Scopes { get; set; } = new Dictionary<string, ScopeConfiguration>();

        public string[] TagPatterns { get; set; } = Array.Empty<string>();

        public string OutputPath { get; set; } = "";

        public string RepositoryPath { get; set; } = null!;

        public Dictionary<string, FooterConfiguration> Footers { get; set; } = new Dictionary<string, FooterConfiguration>();

        public IntegrationsConfiguration Integrations { get; set; } = new IntegrationsConfiguration();

        [ValidationDisplayName("Version Range")]
        public string? VersionRange { get; set; } = "";

        [ValidationDisplayName("Current Version")]
        public string? CurrentVersion { get; set; }

        public TemplateConfiguration Template { get; set; } = new TemplateConfiguration();

        public EntryTypeConfiguration[] EntryTypes { get; set; } = Array.Empty<EntryTypeConfiguration>();

        public ParserConfiguration Parser { get; set; } = new ParserConfiguration();

        public FilterConfiguration Filter { get; set; } = new FilterConfiguration();


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

