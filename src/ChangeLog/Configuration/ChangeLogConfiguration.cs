using System;
using System.Collections.Generic;
using System.IO;

namespace Grynwald.ChangeLog.Configuration
{
    public class ChangeLogConfiguration
    {
        public class ScopeConfiguration
        {
            [SettingDisplayName("Scope Display Name")]
            public string? DisplayName { get; set; }
        }

        public class FooterConfiguration
        {
            [SettingDisplayName("Footer Display Name")]
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
            [SettingDisplayName("GitHub Access Token")]
            public string? AccessToken { get; set; } = null;

            [JsonSchemaDefaultValue]
            [SettingDisplayName("GitHub Remote Name")]
            public string? RemoteName { get; set; } = null;

            [JsonSchemaDefaultValue]
            [SettingDisplayName("GitHub Host")]
            public string? Host { get; set; } = null;

            [JsonSchemaDefaultValue]
            [SettingDisplayName("GitHub Repository Owner")]
            public string? Owner { get; set; } = null;

            [JsonSchemaDefaultValue]
            [SettingDisplayName("GitHub Repository Name")]
            public string? Repository { get; set; } = null;
        }

        public class GitLabIntegrationConfiguration
        {
            [JsonSchemaDefaultValue]
            [SettingDisplayName("GitLab Access Token")]
            public string? AccessToken { get; set; } = null;

            [JsonSchemaDefaultValue]
            [SettingDisplayName("GitLab Remote Name")]
            public string? RemoteName { get; set; } = null;

            [JsonSchemaDefaultValue]
            [SettingDisplayName("GitLab Host")]
            public string? Host { get; set; } = null;

            [JsonSchemaDefaultValue]
            [SettingDisplayName("GitLab Namespace")]
            public string? Namespace { get; set; } = null;

            [JsonSchemaDefaultValue]
            [SettingDisplayName("GitLab Project Name")]
            public string? Project { get; set; } = null;
        }

        public class IntegrationsConfiguration
        {
            [JsonSchemaDefaultValue]
            [SettingDisplayName("Integration Provider")]
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
            GitHubRelease,
            Html,
            Markdown
        }

        public class TemplateConfiguration
        {
            [JsonSchemaDefaultValue]
            [SettingDisplayName("Template Name")]
            public TemplateName Name { get; set; }

            public DefaultTemplateConfiguration Default { get; set; } = new();

            public GitHubReleaseTemplateConfiguration GitHubRelease { get; set; } = new();

            public GitLabReleaseTemplateConfiguration GitLabRelease { get; set; } = new();

            public HtmlTemplateConfiguration Html { get; set; } = new();

            public MarkdownTemplateConfiguration Markdown { get; set; } = new();
        }

        public enum MarkdownPreset
        {
            Default,
            MkDocs
        }

        public class DefaultTemplateConfiguration
        {
            [JsonSchemaDefaultValue]
            [SettingDisplayName("Default Template Markdown Preset")]
            public MarkdownPreset MarkdownPreset { get; set; } = MarkdownPreset.Default;

            [JsonSchemaDefaultValue]
            public bool NormalizeReferences { get; set; } = true;
        }

        public class MarkdownTemplateConfiguration
        {
            [JsonSchemaDefaultValue]
            public bool NormalizeReferences { get; set; } = true;
        }

        public class GitHubReleaseTemplateConfiguration
        {
            [JsonSchemaDefaultValue]
            public bool NormalizeReferences { get; set; } = true;
        }

        public class GitLabReleaseTemplateConfiguration
        {
            [JsonSchemaDefaultValue]
            public bool NormalizeReferences { get; set; } = true;
        }

        public class HtmlTemplateConfiguration
        {
            [JsonSchemaDefaultValue]
            public bool NormalizeReferences { get; set; } = true;
        }

        public class EntryTypeConfiguration
        {
            [SettingDisplayName("Entry Type Display Name")]
            public string? DisplayName { get; set; }

            [SettingDisplayName("Entry Type Priority")]
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
            [SettingDisplayName("Parser Mode")]
            public ParserMode Mode { get; set; }
        }


        public class FilterConfiguration
        {
            [SettingDisplayName("Filter Include Expressions")]
            public FilterExpressionConfiguration[] Include { get; set; } = Array.Empty<FilterExpressionConfiguration>();

            [SettingDisplayName("Filter Exclude Expressions")]
            public FilterExpressionConfiguration[] Exclude { get; set; } = Array.Empty<FilterExpressionConfiguration>();
        }

        public class FilterExpressionConfiguration
        {
            [JsonSchemaDefaultValue]
            [SettingDisplayName("Filter Type Expression")]
            public string Type { get; set; } = "*";

            [JsonSchemaDefaultValue]
            [SettingDisplayName("Filter Scope Expression")]
            public string Scope { get; set; } = "*";
        }


        [JsonSchemaDefaultValue]
        public Dictionary<string, ScopeConfiguration> Scopes { get; set; } = new Dictionary<string, ScopeConfiguration>();

        [JsonSchemaDefaultValue, JsonSchemaUniqueItems]
        [SettingDisplayName("Tag Patterns")]
        public string[] TagPatterns { get; set; } = Array.Empty<string>();

        [JsonSchemaDefaultValue]
        [SettingDisplayName("Output Path")]
        public string OutputPath { get; set; } = "";

        [JsonSchemaIgnore]
        public string RepositoryPath { get; set; } = null!;

        [JsonSchemaDefaultValue]
        public Dictionary<string, FooterConfiguration> Footers { get; set; } = new Dictionary<string, FooterConfiguration>();

        public IntegrationsConfiguration Integrations { get; set; } = new IntegrationsConfiguration();

        [JsonSchemaDefaultValue]
        [SettingDisplayName("Version Range")]
        public string? VersionRange { get; set; } = "";

        [JsonSchemaDefaultValue]
        [SettingDisplayName("Current Version")]
        public string? CurrentVersion { get; set; }

        public TemplateConfiguration Template { get; set; } = new TemplateConfiguration();

        [JsonSchemaDefaultValue]
        public Dictionary<string, EntryTypeConfiguration> EntryTypes { get; set; } = new Dictionary<string, EntryTypeConfiguration>();

        public ParserConfiguration Parser { get; set; } = new ParserConfiguration();

        [JsonSchemaDefaultValue]
        [SettingDisplayName("Filter")]
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

