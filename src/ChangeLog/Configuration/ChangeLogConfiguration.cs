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
            [JsonSchemaDefaultValue]
            [ValidationDisplayName("GitHub Access Token")]
            public string? AccessToken { get; set; } = null;

            [JsonSchemaDefaultValue]
            [ValidationDisplayName("GitHub Remote Name")]
            public string? RemoteName { get; set; } = null;

            [JsonSchemaDefaultValue]
            [ValidationDisplayName("GitHub Host")]
            public string? Host { get; set; } = null;

            [JsonSchemaDefaultValue]
            [ValidationDisplayName("GitHub Owner Name")]
            public string? Owner { get; set; } = null;

            [JsonSchemaDefaultValue]
            [ValidationDisplayName("GitHub Repository Name")]
            public string? Repository { get; set; } = null;
        }

        public class GitLabIntegrationConfiguration
        {
            [JsonSchemaDefaultValue]
            [ValidationDisplayName("GitLab Access Token")]
            public string? AccessToken { get; set; } = null;

            [JsonSchemaDefaultValue]
            [ValidationDisplayName("GitLab Remote Name")]
            public string? RemoteName { get; set; } = null;

            [JsonSchemaDefaultValue]
            [ValidationDisplayName("GitLab Host")]
            public string? Host { get; set; } = null;

            [JsonSchemaDefaultValue]
            [ValidationDisplayName("GitLab Namespace")]
            public string? Namespace { get; set; } = null;

            [JsonSchemaDefaultValue]
            [ValidationDisplayName("GitLab Project Name")]
            public string? Project { get; set; } = null;
        }

        public class IntegrationsConfiguration
        {
            [JsonSchemaDefaultValue]
            public IntegrationProvider Provider { get; set; }

            [JsonSchemaPropertyName("github")]
            public GitHubIntegrationConfiguration GitHub { get; set; } = new GitHubIntegrationConfiguration();

            [JsonSchemaPropertyName("gitlab")]
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
            [JsonSchemaDefaultValue]
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
            [JsonSchemaDefaultValue]
            public MarkdownPreset MarkdownPreset { get; set; } = MarkdownPreset.Default;
        }

        public class EntryTypeConfiguration
        {
            public string? DisplayName { get; set; }

            public int Priority { get; set; }
        }

        public enum ParserMode
        {
            Strict,
            Loose
        }

        public class ParserConfiguration
        {
            [JsonSchemaDefaultValue]
            public ParserMode Mode { get; set; }
        }


        public class FilterConfiguration
        {
            public FilterExpressionConfiguration[] Include { get; set; } = Array.Empty<FilterExpressionConfiguration>();

            public FilterExpressionConfiguration[] Exclude { get; set; } = Array.Empty<FilterExpressionConfiguration>();
        }

        public class FilterExpressionConfiguration
        {
            [JsonSchemaDefaultValue]
            [ValidationDisplayName("Filter Type Expression")]
            public string Type { get; set; } = "*";

            [JsonSchemaDefaultValue]
            [ValidationDisplayName("Filter Scope Expression")]
            public string Scope { get; set; } = "*";
        }


        [JsonSchemaDefaultValue]
        public Dictionary<string, ScopeConfiguration> Scopes { get; set; } = new Dictionary<string, ScopeConfiguration>();

        [JsonSchemaDefaultValue, JsonSchemaUniqueItems]
        public string[] TagPatterns { get; set; } = Array.Empty<string>();

        [JsonSchemaDefaultValue]
        public string OutputPath { get; set; } = "";

        [JsonSchemaIgnore]
        public string RepositoryPath { get; set; } = null!;

        [JsonSchemaDefaultValue]
        public Dictionary<string, FooterConfiguration> Footers { get; set; } = new Dictionary<string, FooterConfiguration>();

        public IntegrationsConfiguration Integrations { get; set; } = new IntegrationsConfiguration();

        [JsonSchemaDefaultValue]
        [ValidationDisplayName("Version Range")]
        public string? VersionRange { get; set; } = "";

        [JsonSchemaDefaultValue]
        [ValidationDisplayName("Current Version")]
        public string? CurrentVersion { get; set; }

        public TemplateConfiguration Template { get; set; } = new TemplateConfiguration();

        [JsonSchemaDefaultValue]
        public Dictionary<string, EntryTypeConfiguration> EntryTypes { get; set; } = new Dictionary<string, EntryTypeConfiguration>();

        public ParserConfiguration Parser { get; set; } = new ParserConfiguration();

        [JsonSchemaDefaultValue]
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

