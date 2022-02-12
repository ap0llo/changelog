using System;
using System.Collections.Generic;
using Grynwald.ChangeLog.Configuration;
using Grynwald.Utilities.IO;
using Xunit;

namespace Grynwald.ChangeLog.Test.Configuration
{
    /// <summary>
    /// Tests for <see cref="ConfigurationValidator"/>
    /// </summary>
    public class ConfigurationValidatorTest
    {
        [Fact]
        public void Validate_checks_arguments_for_null()
        {
            var sut = new ConfigurationValidator();
            Assert.Throws<ArgumentNullException>(() => sut.Validate((ChangeLogConfiguration)null!));
        }

        [Fact]
        public void No_errors_are_found_in_default_configuration()
        {
            // ARRANGE
            var sut = new ConfigurationValidator();

            // ACT 
            var result = sut.Validate(ChangeLogConfigurationLoader.GetDefaultConfiguration());

            // ASSERT
            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData("")]
        [InlineData("\t")]
        [InlineData("  ")]
        public void Scope_name_must_not_be_empty_or_whitespace(string scopeName)
        {
            // ARRANGE
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            config.Scopes = new Dictionary<string, ChangeLogConfiguration.ScopeConfiguration>()
            {
                { scopeName, new ChangeLogConfiguration.ScopeConfiguration(){ DisplayName = "Display Name"} }
            };

            var sut = new ConfigurationValidator();

            // ACT 
            var result = sut.Validate(config);

            // ASSERT
            Assert.False(result.IsValid);
            var error = Assert.Single(result.Errors);
            Assert.Contains("'Scope Name'", error.ErrorMessage);
        }

        [Theory]
        [InlineData("some-scope")]
        public void Scope_name_must_be_unique(string scopeName)
        {
            // ARRANGE
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            config.Scopes = new Dictionary<string, ChangeLogConfiguration.ScopeConfiguration>()
            {
                { scopeName.ToLower(), new ChangeLogConfiguration.ScopeConfiguration() },
                { scopeName.ToUpper(), new ChangeLogConfiguration.ScopeConfiguration() }
            };

            var sut = new ConfigurationValidator();

            // ACT 
            var result = sut.Validate(config);

            // ASSERT
            Assert.False(result.IsValid);
            var error = Assert.Single(result.Errors);
            Assert.Contains("'Scope Name' must be unique", error.ErrorMessage);
        }

        [Theory]
        [InlineData("")]
        [InlineData("\t")]
        [InlineData("  ")]
        public void Footer_name_must_not_be_empty_or_whitespace(string footerName)
        {
            // ARRANGE
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            config.Footers = new Dictionary<string, ChangeLogConfiguration.FooterConfiguration>()
            {
                { footerName, new ChangeLogConfiguration.FooterConfiguration() { DisplayName = "Display Name"} }
            };

            var sut = new ConfigurationValidator();

            // ACT 
            var result = sut.Validate(config);

            // ASSERT
            Assert.False(result.IsValid);
            var error = Assert.Single(result.Errors);
            Assert.Contains("'Footer Name'", error.ErrorMessage);
        }

        [Theory]
        [InlineData("some-footer")]
        public void Footer_name_must_be_unique(string footerName)
        {
            // ARRANGE
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            config.Footers = new Dictionary<string, ChangeLogConfiguration.FooterConfiguration>()
            {
                { footerName.ToLower(), new ChangeLogConfiguration.FooterConfiguration() },
                { footerName.ToUpper(), new ChangeLogConfiguration.FooterConfiguration() }
            };

            var sut = new ConfigurationValidator();

            // ACT 
            var result = sut.Validate(config);

            // ASSERT
            Assert.False(result.IsValid);
            var error = Assert.Single(result.Errors);
            Assert.Contains("'Footer Name' must be unique", error.ErrorMessage);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void VersionRange_can_be_null_or_empty(string versionRange)
        {
            // ARRANGE
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            config.VersionRange = versionRange;

            var sut = new ConfigurationValidator();

            // ACT 
            var result = sut.Validate(config);

            // ASSERT
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Theory]
        [InlineData("\t")]
        [InlineData("  ")]
        [InlineData("not-a-version-range")]
        public void VersionRange_must_be_valid_if_set(string versionRange)
        {
            // ARRANGE
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            config.VersionRange = versionRange;

            var sut = new ConfigurationValidator();

            // ACT 
            var result = sut.Validate(config);

            // ASSERT
            Assert.False(result.IsValid);
            Assert.All(result.Errors, error => Assert.Contains("'Version Range'", error.ErrorMessage));
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void CurrentVersion_can_be_null_or_empty(string currentVersion)
        {
            // ARRANGE
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            config.CurrentVersion = currentVersion;

            var sut = new ConfigurationValidator();

            // ACT 
            var result = sut.Validate(config);

            // ASSERT
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Theory]
        [InlineData("\t")]
        [InlineData("  ")]
        [InlineData("not-a-version-range")]
        public void CurrentVersion_must_be_valid_if_set(string currentVersion)
        {
            // ARRANGE
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            config.CurrentVersion = currentVersion;

            var sut = new ConfigurationValidator();

            // ACT 
            var result = sut.Validate(config);

            // ASSERT
            Assert.False(result.IsValid);
            Assert.All(result.Errors, error => Assert.Contains("'Current Version'", error.ErrorMessage));
        }

        [Theory]
        [InlineData("")]
        [InlineData("\t")]
        [InlineData("  ")]
        public void Entry_type_must_not_be_empty_or_whitespace(string entryType)
        {
            // ARRANGE
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            config.EntryTypes = new Dictionary<string, ChangeLogConfiguration.EntryTypeConfiguration>()
            {
                { entryType, new ChangeLogConfiguration.EntryTypeConfiguration() { DisplayName = "Display Name" } }
            };

            var sut = new ConfigurationValidator();

            // ACT 
            var result = sut.Validate(config);

            // ASSERT
            Assert.False(result.IsValid);
            var error = Assert.Single(result.Errors);
            Assert.Contains("'Entry Type'", error.ErrorMessage);
        }

        [Theory]
        [InlineData("feat")]
        public void Entry_types_must_not_be_unique(string entryType)
        {
            // ARRANGE
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            config.EntryTypes = new Dictionary<string, ChangeLogConfiguration.EntryTypeConfiguration>()
            {
                { entryType.ToLower(), new ChangeLogConfiguration.EntryTypeConfiguration() { DisplayName = "Display Name" } },
                { entryType.ToUpper(), new ChangeLogConfiguration.EntryTypeConfiguration() { DisplayName = "Display Name" } }
            };

            var sut = new ConfigurationValidator();

            // ACT 
            var result = sut.Validate(config);

            // ASSERT
            Assert.False(result.IsValid);
            var error = Assert.Single(result.Errors);
            Assert.Contains("'Entry Type' must be unique", error.ErrorMessage);
        }

        [Fact]
        public void Integrations_Provider_must_be_defined_value()
        {
            // ARRANGE
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            config.Integrations.Provider = (ChangeLogConfiguration.IntegrationProvider)(-1);

            var sut = new ConfigurationValidator();

            // ACT 
            var result = sut.Validate(config);

            // ASSERT
            Assert.False(result.IsValid);
            Assert.Collection(result.Errors,
                error => Assert.Contains("'Integration Provider'", error.ErrorMessage)
            );
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void GitHub_AccessToken_can_be_null_or_empty(string accessToken)
        {
            // ARRANGE
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            config.Integrations.GitHub.AccessToken = accessToken;

            var sut = new ConfigurationValidator();

            // ACT 
            var result = sut.Validate(config);

            // ASSERT
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Theory]
        [InlineData("\t")]
        [InlineData("  ")]
        public void GitHub_AccessToken_must_not_be_whitespace(string accessToken)
        {
            // ARRANGE
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            config.Integrations.GitHub.AccessToken = accessToken;

            var sut = new ConfigurationValidator();

            // ACT 
            var result = sut.Validate(config);

            // ASSERT
            Assert.False(result.IsValid);
            var error = Assert.Single(result.Errors);
            Assert.Contains("'GitHub Access Token'", error.ErrorMessage);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("\t")]
        [InlineData("  ")]
        public void GitHub_RemoteName_must_not_be_null_or_whitespace(string remoteName)
        {
            // ARRANGE
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            config.Integrations.GitHub.RemoteName = remoteName;

            var sut = new ConfigurationValidator();

            // ACT 
            var result = sut.Validate(config);

            // ASSERT
            Assert.False(result.IsValid);
            var error = Assert.Single(result.Errors);
            Assert.Contains("'GitHub Remote Name'", error.ErrorMessage);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void GitHub_Host_can_be_null_or_empty(string host)
        {
            // ARRANGE
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            config.Integrations.GitHub.Host = host;

            var sut = new ConfigurationValidator();

            // ACT 
            var result = sut.Validate(config);

            // ASSERT
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Theory]
        [InlineData("\t")]
        [InlineData("  ")]
        public void GitHub_Host_must_not_be_whitespace(string host)
        {
            // ARRANGE
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            config.Integrations.GitHub.Host = host;

            var sut = new ConfigurationValidator();

            // ACT 
            var result = sut.Validate(config);

            // ASSERT
            Assert.False(result.IsValid);
            var error = Assert.Single(result.Errors);
            Assert.Contains("'GitHub Host'", error.ErrorMessage);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void GitHub_Owner_can_be_null_or_empty(string owner)
        {
            // ARRANGE
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            config.Integrations.GitHub.Owner = owner;

            var sut = new ConfigurationValidator();

            // ACT 
            var result = sut.Validate(config);

            // ASSERT
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Theory]
        [InlineData("\t")]
        [InlineData("  ")]
        public void GitHub_Owner_must_not_be_whitespace(string owner)
        {
            // ARRANGE
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            config.Integrations.GitHub.Owner = owner;

            var sut = new ConfigurationValidator();

            // ACT 
            var result = sut.Validate(config);

            // ASSERT
            Assert.False(result.IsValid);
            var error = Assert.Single(result.Errors);
            Assert.Contains("'GitHub Repository Owner'", error.ErrorMessage);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void GitHub_Repository_can_be_null_or_empty(string repository)
        {
            // ARRANGE
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            config.Integrations.GitHub.Repository = repository;

            var sut = new ConfigurationValidator();

            // ACT 
            var result = sut.Validate(config);

            // ASSERT
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Theory]
        [InlineData("\t")]
        [InlineData("  ")]
        public void GitHub_Repository_must_not_be_whitespace(string repository)
        {
            // ARRANGE
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            config.Integrations.GitHub.Repository = repository;

            var sut = new ConfigurationValidator();

            // ACT 
            var result = sut.Validate(config);

            // ASSERT
            Assert.False(result.IsValid);
            var error = Assert.Single(result.Errors);
            Assert.Contains("'GitHub Repository Name'", error.ErrorMessage);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void GitLab_AccessToken_can_be_null_or_empty(string accessToken)
        {
            // ARRANGE
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            config.Integrations.GitLab.AccessToken = accessToken;

            var sut = new ConfigurationValidator();

            // ACT 
            var result = sut.Validate(config);

            // ASSERT
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Theory]
        [InlineData("\t")]
        [InlineData("  ")]
        public void GitLab_AccessToken_must_not_be_whitespace(string accessToken)
        {
            // ARRANGE
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            config.Integrations.GitLab.AccessToken = accessToken;

            var sut = new ConfigurationValidator();

            // ACT 
            var result = sut.Validate(config);

            // ASSERT
            Assert.False(result.IsValid);
            var error = Assert.Single(result.Errors);
            Assert.Contains("'GitLab Access Token'", error.ErrorMessage);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("\t")]
        [InlineData("  ")]
        public void GitLab_RemoteName_must_not_be_null_or_whitespace(string remoteName)
        {
            // ARRANGE
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            config.Integrations.GitLab.RemoteName = remoteName;

            var sut = new ConfigurationValidator();

            // ACT 
            var result = sut.Validate(config);

            // ASSERT
            Assert.False(result.IsValid);
            var error = Assert.Single(result.Errors);
            Assert.Contains("'GitLab Remote Name'", error.ErrorMessage);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void GitLab_Host_can_be_null_or_empty(string host)
        {
            // ARRANGE
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            config.Integrations.GitLab.Host = host;

            var sut = new ConfigurationValidator();

            // ACT 
            var result = sut.Validate(config);

            // ASSERT
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Theory]
        [InlineData("\t")]
        [InlineData("  ")]
        public void GitLab_Host_must_not_be_whitespace(string host)
        {
            // ARRANGE
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            config.Integrations.GitLab.Host = host;

            var sut = new ConfigurationValidator();

            // ACT 
            var result = sut.Validate(config);

            // ASSERT
            Assert.False(result.IsValid);
            var error = Assert.Single(result.Errors);
            Assert.Contains("'GitLab Host'", error.ErrorMessage);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void GitLab_Namespace_can_be_null_or_empty(string @namespace)
        {
            // ARRANGE
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            config.Integrations.GitLab.Namespace = @namespace;

            var sut = new ConfigurationValidator();

            // ACT 
            var result = sut.Validate(config);

            // ASSERT
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Theory]
        [InlineData("\t")]
        [InlineData("  ")]
        public void GitLab_Namespace_must_not_be_whitespace(string @namespace)
        {
            // ARRANGE
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            config.Integrations.GitLab.Namespace = @namespace;

            var sut = new ConfigurationValidator();

            // ACT 
            var result = sut.Validate(config);

            // ASSERT
            Assert.False(result.IsValid);
            var error = Assert.Single(result.Errors);
            Assert.Contains("'GitLab Namespace'", error.ErrorMessage);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void GitLab_Project_can_be_null_or_empty(string project)
        {
            // ARRANGE
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            config.Integrations.GitLab.Project = project;

            var sut = new ConfigurationValidator();

            // ACT 
            var result = sut.Validate(config);

            // ASSERT
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Theory]
        [InlineData("\t")]
        [InlineData("  ")]
        public void GitLab_Project_must_not_be_whitespace(string project)
        {
            // ARRANGE
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            config.Integrations.GitLab.Project = project;

            var sut = new ConfigurationValidator();

            // ACT 
            var result = sut.Validate(config);

            // ASSERT
            Assert.False(result.IsValid);
            var error = Assert.Single(result.Errors);
            Assert.Contains("'GitLab Project Name'", error.ErrorMessage);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Filter_Type_expression_can_be_null_or_empty(string filterType)
        {
            // ARRANGE
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            config.Filter.Include = new[]
            {
                new ChangeLogConfiguration.FilterExpressionConfiguration()
                {
                    Type = filterType
                }
            };
            config.Filter.Exclude = new[]
            {
                new ChangeLogConfiguration.FilterExpressionConfiguration()
                {
                    Type = filterType
                }
            };

            var sut = new ConfigurationValidator();

            // ACT 
            var result = sut.Validate(config);

            // ASSERT
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Theory]
        [InlineData("\t")]
        [InlineData("  ")]
        public void Filter_Type_expression_must_not_be_whitespace(string filterType)
        {
            // ARRANGE
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            config.Filter.Include = new[]
            {
                new ChangeLogConfiguration.FilterExpressionConfiguration()
                {
                    Type = filterType
                }
            };
            config.Filter.Exclude = new[]
            {
                new ChangeLogConfiguration.FilterExpressionConfiguration()
                {
                    Type = filterType
                }
            };
            var sut = new ConfigurationValidator();

            // ACT 
            var result = sut.Validate(config);

            // ASSERT
            Assert.False(result.IsValid);
            Assert.Collection(result.Errors,
                error => Assert.Contains("'Filter Type Expression'", error.ErrorMessage),
                error => Assert.Contains("'Filter Type Expression'", error.ErrorMessage)
            );
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Filter_Scope_expression_can_be_null_or_empty(string filterType)
        {
            // ARRANGE
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            config.Filter.Include = new[]
            {
                new ChangeLogConfiguration.FilterExpressionConfiguration()
                {
                    Scope = filterType
                }
            };
            config.Filter.Exclude = new[]
            {
                new ChangeLogConfiguration.FilterExpressionConfiguration()
                {
                    Scope = filterType
                }
            };

            var sut = new ConfigurationValidator();

            // ACT 
            var result = sut.Validate(config);

            // ASSERT
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Theory]
        [InlineData("\t")]
        [InlineData("  ")]
        public void Filter_Scope_expression_must_not_be_whitespace(string filterType)
        {
            // ARRANGE
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            config.Filter.Include = new[]
            {
                new ChangeLogConfiguration.FilterExpressionConfiguration()
                {
                    Scope = filterType
                }
            };
            config.Filter.Exclude = new[]
            {
                new ChangeLogConfiguration.FilterExpressionConfiguration()
                {
                    Scope = filterType
                }
            };
            var sut = new ConfigurationValidator();

            // ACT 
            var result = sut.Validate(config);

            // ASSERT
            Assert.False(result.IsValid);
            Assert.Collection(result.Errors,
                error => Assert.Contains("'Filter Scope Expression'", error.ErrorMessage),
                error => Assert.Contains("'Filter Scope Expression'", error.ErrorMessage)
            );
        }

        [Fact]
        public void Template_Name_must_be_defined_value()
        {
            // ARRANGE
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            config.Template.Name = (ChangeLogConfiguration.TemplateName)(-1);

            var sut = new ConfigurationValidator();

            // ACT 
            var result = sut.Validate(config);

            // ASSERT
            Assert.False(result.IsValid);
            Assert.Collection(result.Errors,
                error => Assert.Contains("'Template Name'", error.ErrorMessage)
            );
        }

        [Theory]
        [CombinatorialData]
        public void Template_custom_directory_can_be_null_or_empty(
            ChangeLogConfiguration.TemplateName template,
            [CombinatorialValues(null, "")] string customDirectory)
        {
            // ARRANGE
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();

            var customDirectoryProperty = config.Template.GetType().GetProperty(template.ToString());
            var templateSettings = (ChangeLogConfiguration.TemplateSettings)customDirectoryProperty!.GetValue(config.Template)!;
            templateSettings.CustomDirectory = customDirectory;

            var sut = new ConfigurationValidator();

            // ACT 
            var result = sut.Validate(config);

            // ASSERT
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Theory]
        [CombinatorialData]
        public void Template_custom_directory_must_not_be_whitespace(
            ChangeLogConfiguration.TemplateName template,
            [CombinatorialValues("\t", " ")] string customDirectory)
        {
            // ARRANGE
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();

            var customDirectoryProperty = config.Template.GetType().GetProperty(template.ToString());
            var templateSettings = (ChangeLogConfiguration.TemplateSettings)customDirectoryProperty!.GetValue(config.Template)!;
            templateSettings.CustomDirectory = customDirectory;


            var sut = new ConfigurationValidator();

            // ACT 
            var result = sut.Validate(config);

            // ASSERT
            Assert.False(result.IsValid);
            Assert.Collection(result.Errors,
                error => Assert.Contains("'Template Custom Directory'", error.ErrorMessage)
            );
        }

        [Theory]
        [CombinatorialData]
        public void Template_custom_directory_must_exist_when_it_is_not_null_or_empty(ChangeLogConfiguration.TemplateName template)
        {
            // ARRANGE
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();

            var customDirectoryProperty = config.Template.GetType().GetProperty(template.ToString());
            var templateSettings = (ChangeLogConfiguration.TemplateSettings)customDirectoryProperty!.GetValue(config.Template)!;
            templateSettings.CustomDirectory = "/Some-Directory";


            var sut = new ConfigurationValidator();

            // ACT 
            var result = sut.Validate(config);

            // ASSERT
            Assert.False(result.IsValid);
            Assert.Collection(result.Errors,
                error =>
                {
                    Assert.Contains("'Template Custom Directory'", error.ErrorMessage);
                    Assert.Contains("'/Some-Directory'", error.ErrorMessage);
                }
            );
        }

        [Theory]
        [CombinatorialData]
        public void Template_custom_directory_is_valid_when_directory_exists(ChangeLogConfiguration.TemplateName template)
        {
            // ARRANGE
            using var temporaryDirectory = new TemporaryDirectory();

            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();

            var customDirectoryProperty = config.Template.GetType().GetProperty(template.ToString());
            var templateSettings = (ChangeLogConfiguration.TemplateSettings)customDirectoryProperty!.GetValue(config.Template)!;
            templateSettings.CustomDirectory = temporaryDirectory;

            var sut = new ConfigurationValidator();

            // ACT 
            var result = sut.Validate(config);

            // ASSERT
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void Parser_Mode_must_be_defined_value()
        {
            // ARRANGE
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            config.Parser.Mode = (ChangeLogConfiguration.ParserMode)(-1);

            var sut = new ConfigurationValidator();

            // ACT 
            var result = sut.Validate(config);

            // ASSERT
            Assert.False(result.IsValid);
            Assert.Collection(result.Errors,
                error => Assert.Contains("'Parser Mode'", error.ErrorMessage)
            );
        }

        [Fact]
        public void MessageOverrides_Provider_must_be_defined_value()
        {
            // ARRANGE
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            config.MessageOverrides.Provider = (ChangeLogConfiguration.MessageOverrideProvider)(-1);

            var sut = new ConfigurationValidator();

            // ACT 
            var result = sut.Validate(config);

            // ASSERT
            Assert.False(result.IsValid);
            Assert.Collection(result.Errors,
                error => Assert.Contains("'Commit Message Override Provider'", error.ErrorMessage)
            );
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("\t")]
        [InlineData("  ")]
        public void MessageOverrides_GitNotesNamespace_must_not_be_null_or_whitespace_when_message_overrides_are_enabled(string gitNotesNamespace)
        {
            // ARRANGE
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            config.MessageOverrides.Enabled = true;
            config.MessageOverrides.GitNotesNamespace = gitNotesNamespace;

            var sut = new ConfigurationValidator();

            // ACT 
            var result = sut.Validate(config);

            // ASSERT
            Assert.False(result.IsValid);
            Assert.Collection(result.Errors,
                error => Assert.Contains("'Commit Message Overide Git Notes Namespace'", error.ErrorMessage)
            );
        }

        [Theory]
        [InlineData(false, ChangeLogConfiguration.MessageOverrideProvider.GitNotes, null)]
        [InlineData(false, ChangeLogConfiguration.MessageOverrideProvider.GitNotes, "")]
        [InlineData(false, ChangeLogConfiguration.MessageOverrideProvider.GitNotes, "\t")]
        [InlineData(false, ChangeLogConfiguration.MessageOverrideProvider.GitNotes, "  ")]
        [InlineData(false, ChangeLogConfiguration.MessageOverrideProvider.FileSystem, null)]
        [InlineData(false, ChangeLogConfiguration.MessageOverrideProvider.FileSystem, "")]
        [InlineData(false, ChangeLogConfiguration.MessageOverrideProvider.FileSystem, "\t")]
        [InlineData(false, ChangeLogConfiguration.MessageOverrideProvider.FileSystem, "  ")]
        [InlineData(true, ChangeLogConfiguration.MessageOverrideProvider.FileSystem, null)]
        [InlineData(true, ChangeLogConfiguration.MessageOverrideProvider.FileSystem, "")]
        [InlineData(true, ChangeLogConfiguration.MessageOverrideProvider.FileSystem, "\t")]
        [InlineData(true, ChangeLogConfiguration.MessageOverrideProvider.FileSystem, "  ")]
        public void MessageOverrides_GitNotesNamespace_may_be_null_or_whitespace_when_message_overrides_are_disabled_or_another_provider_is_used(bool enabled, ChangeLogConfiguration.MessageOverrideProvider provider, string gitNotesNamespace)
        {
            // ARRANGE
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            config.MessageOverrides.Enabled = enabled;
            config.MessageOverrides.Provider = provider;
            config.MessageOverrides.GitNotesNamespace = gitNotesNamespace;

            var sut = new ConfigurationValidator();

            // ACT 
            var result = sut.Validate(config);

            // ASSERT
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }


        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("\t")]
        [InlineData("  ")]
        public void MessageOverrides_SourceDirectoryPath_must_not_be_null_or_whitespace_when_message_overrides_are_enabled(string sourceDirectoryPath)
        {
            // ARRANGE
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            config.MessageOverrides.Enabled = true;
            config.MessageOverrides.Provider = ChangeLogConfiguration.MessageOverrideProvider.FileSystem;
            config.MessageOverrides.SourceDirectoryPath = sourceDirectoryPath;

            var sut = new ConfigurationValidator();

            // ACT 
            var result = sut.Validate(config);

            // ASSERT
            Assert.False(result.IsValid);
            Assert.Collection(result.Errors,
                error => Assert.Contains("'Commit Message Override Source Directory'", error.ErrorMessage)
            );
        }

        [Theory]
        [InlineData(true, ChangeLogConfiguration.MessageOverrideProvider.GitNotes, null)]
        [InlineData(true, ChangeLogConfiguration.MessageOverrideProvider.GitNotes, "")]
        [InlineData(true, ChangeLogConfiguration.MessageOverrideProvider.GitNotes, "\t")]
        [InlineData(true, ChangeLogConfiguration.MessageOverrideProvider.GitNotes, "  ")]
        [InlineData(false, ChangeLogConfiguration.MessageOverrideProvider.GitNotes, null)]
        [InlineData(false, ChangeLogConfiguration.MessageOverrideProvider.GitNotes, "")]
        [InlineData(false, ChangeLogConfiguration.MessageOverrideProvider.GitNotes, "\t")]
        [InlineData(false, ChangeLogConfiguration.MessageOverrideProvider.GitNotes, "  ")]
        [InlineData(false, ChangeLogConfiguration.MessageOverrideProvider.FileSystem, null)]
        [InlineData(false, ChangeLogConfiguration.MessageOverrideProvider.FileSystem, "")]
        [InlineData(false, ChangeLogConfiguration.MessageOverrideProvider.FileSystem, "\t")]
        [InlineData(false, ChangeLogConfiguration.MessageOverrideProvider.FileSystem, "  ")]
        public void MessageOverrides_SourceDirectoryPath_may_be_null_or_whitespace_when_message_overrides_are_disabled_or_another_provider_is_used(bool enabled, ChangeLogConfiguration.MessageOverrideProvider provider, string sourceDirectoryPath)
        {
            // ARRANGE
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            config.MessageOverrides.Enabled = enabled;
            config.MessageOverrides.Provider = provider;
            config.MessageOverrides.SourceDirectoryPath = sourceDirectoryPath;

            var sut = new ConfigurationValidator();

            // ACT 
            var result = sut.Validate(config);

            // ASSERT
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

    }
}
