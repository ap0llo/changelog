using System;
using Grynwald.ChangeLog.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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
        [InlineData(null)]
        public void Scope_name_must_not_be_null_of_whitespace(string scopeName)
        {
            // ARRANGE
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            config.Scopes = new[]
            {
                new ChangeLogConfiguration.ScopeConfiguration(){ Name = scopeName, DisplayName = "Display Name"}
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
        [InlineData("")]
        [InlineData("\t")]
        [InlineData("  ")]
        [InlineData(null)]
        public void Footer_name_must_not_be_null_of_whitespace(string footerName)
        {
            // ARRANGE
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            config.Footers = new[]
            {
                new ChangeLogConfiguration.FooterConfiguration(){ Name = footerName, DisplayName = "Display Name"}
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
        [InlineData(null)]
        public void Entry_type_must_not_be_null_of_whitespace(string entryType)
        {
            // ARRANGE
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            config.EntryTypes = new[]
            {
                new ChangeLogConfiguration.EntryTypeConfiguration(){ Type = entryType, DisplayName = "Display Name"}
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
            Assert.Contains("'GitHub Owner Name'", error.ErrorMessage);
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
    }
}
