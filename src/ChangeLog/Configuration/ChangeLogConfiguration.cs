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
            [JsonSchemaTitle("Scope Display Name")]
            public string? DisplayName { get; set; }
        }

        public class FooterConfiguration
        {
            [JsonSchemaTitle("Footer Display Name")]
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
            [JsonSchemaDefaultValue, JsonSchemaTitle("GitHub Access Token")]
            [ValidationDisplayName("GitHub Access Token")]
            public string? AccessToken { get; set; } = null;

            [JsonSchemaDefaultValue, JsonSchemaTitle("GitHub Remote Name")]
            [ValidationDisplayName("GitHub Remote Name")]
            public string? RemoteName { get; set; } = null;

            [JsonSchemaDefaultValue, JsonSchemaTitle("GitHub Host")]
            [ValidationDisplayName("GitHub Host")]
            public string? Host { get; set; } = null;

            [ValidationDisplayName("GitHub Owner Name")]
            [JsonSchemaDefaultValue, JsonSchemaTitle("GitHub Repository Owner")]
            public string? Owner { get; set; } = null;

            [JsonSchemaDefaultValue, JsonSchemaTitle("GitHub Repository Name")]
            [ValidationDisplayName("GitHub Repository Name")]
            public string? Repository { get; set; } = null;
        }

        public class GitLabIntegrationConfiguration
        {
            [JsonSchemaDefaultValue, JsonSchemaTitle("GitLab Access Token")]
            [ValidationDisplayName("GitLab Access Token")]
            public string? AccessToken { get; set; } = null;

            [JsonSchemaDefaultValue, JsonSchemaTitle("GitLab Remote Name")]
            [ValidationDisplayName("GitLab Remote Name")]
            public string? RemoteName { get; set; } = null;

            [JsonSchemaDefaultValue, JsonSchemaTitle("GitLab Host")]
            [ValidationDisplayName("GitLab Host")]
            public string? Host { get; set; } = null;

            [JsonSchemaDefaultValue, JsonSchemaTitle("GitLab Namespace")]
            [ValidationDisplayName("GitLab Namespace")]
            public string? Namespace { get; set; } = null;

            [JsonSchemaDefaultValue, JsonSchemaTitle("GitLab Project Name")]
            [ValidationDisplayName("GitLab Project Name")]
            public string? Project { get; set; } = null;
        }

        public class IntegrationsConfiguration
        {
            [JsonSchemaDefaultValue, JsonSchemaTitle("Integration Provider")]
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
            [JsonSchemaDefaultValue, JsonSchemaTitle("Template Name")]
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
            [JsonSchemaDefaultValue, JsonSchemaTitle("Default Template Markdown Preset")]
            public MarkdownPreset MarkdownPreset { get; set; } = MarkdownPreset.Default;
        }

        public class EntryTypeConfiguration
        {
            [JsonSchemaTitle("Entry Type Display Name")]
            public string? DisplayName { get; set; }

            [JsonSchemaTitle("Entry Type Priority")]
            public int Priority { get; set; }
        }

        public enum ParserMode
        {
            Strict,
            Loose
        }

        public class ParserConfiguration
        {
            [JsonSchemaDefaultValue, JsonSchemaTitle("Parser Mode")]
            public ParserMode Mode { get; set; }
        }


        public class FilterConfiguration
        {
            [JsonSchemaTitle("Filter Include Expressions")]
            public FilterExpressionConfiguration[] Include { get; set; } = Array.Empty<FilterExpressionConfiguration>();

            [JsonSchemaTitle("Filter Exclude Expressions")]
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

        [JsonSchemaDefaultValue, JsonSchemaUniqueItems, JsonSchemaTitle("Tag Patterns")]
        public string[] TagPatterns { get; set; } = Array.Empty<string>();

        [JsonSchemaDefaultValue, JsonSchemaTitle("Output Path")]
        public string OutputPath { get; set; } = "";

        [JsonSchemaIgnore]
        public string RepositoryPath { get; set; } = null!;

        [JsonSchemaDefaultValue]
        public Dictionary<string, FooterConfiguration> Footers { get; set; } = new Dictionary<string, FooterConfiguration>();

        public IntegrationsConfiguration Integrations { get; set; } = new IntegrationsConfiguration();

        [JsonSchemaDefaultValue, JsonSchemaTitle("Version Range")]
        [ValidationDisplayName("Version Range")]
        public string? VersionRange { get; set; } = "";

        [JsonSchemaDefaultValue, JsonSchemaTitle("Current Version")]
        [ValidationDisplayName("Current Version")]
        public string? CurrentVersion { get; set; }

        public TemplateConfiguration Template { get; set; } = new TemplateConfiguration();

        [JsonSchemaDefaultValue]
        public Dictionary<string, EntryTypeConfiguration> EntryTypes { get; set; } = new Dictionary<string, EntryTypeConfiguration>();

        public ParserConfiguration Parser { get; set; } = new ParserConfiguration();

        [JsonSchemaDefaultValue, JsonSchemaTitle("Filter")]
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

