using Grynwald.ChangeLog.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Grynwald.ChangeLog.Test.Configuration
{
    public class ConfigurationValidatorTest
    {
        private readonly ILogger<ConfigurationValidator> m_Logger = NullLogger<ConfigurationValidator>.Instance;

        [Fact]
        public void No_errors_are_found_in_default_configuration()
        {
            // ARRANGE
            var sut = new ConfigurationValidator(m_Logger);

            // ACT 
            var valid = sut.Validate(ChangeLogConfigurationLoader.GetDefaultConfiguration());

            // ASSERT
            Assert.True(valid);
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

            var sut = new ConfigurationValidator(m_Logger);

            // ACT 
            var valid = sut.Validate(config);

            // ASSERT
            Assert.False(valid);
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

            var sut = new ConfigurationValidator(m_Logger);

            // ACT 
            var valid = sut.Validate(config);

            // ASSERT
            Assert.False(valid);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void VersionRange_can_be_null_or_empty(string versionRange)
        {
            // ARRANGE
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            config.VersionRange = versionRange;

            var sut = new ConfigurationValidator(m_Logger);

            // ACT 
            var valid = sut.Validate(config);

            // ASSERT
            Assert.True(valid);
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

            var sut = new ConfigurationValidator(m_Logger);

            // ACT 
            var valid = sut.Validate(config);

            // ASSERT
            Assert.False(valid);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void CurrentVersion_can_be_null_or_empty(string currentVersion)
        {
            // ARRANGE
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            config.CurrentVersion = currentVersion;

            var sut = new ConfigurationValidator(m_Logger);

            // ACT 
            var valid = sut.Validate(config);

            // ASSERT
            Assert.True(valid);
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

            var sut = new ConfigurationValidator(m_Logger);

            // ACT 
            var valid = sut.Validate(config);

            // ASSERT
            Assert.False(valid);
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

            var sut = new ConfigurationValidator(m_Logger);

            // ACT 
            var valid = sut.Validate(config);

            // ASSERT
            Assert.False(valid);
        }


        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void GitHubAccessToken_can_be_null_or_empty(string accessToken)
        {
            // ARRANGE
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            config.Integrations.GitHub.AccessToken = accessToken;

            var sut = new ConfigurationValidator(m_Logger);

            // ACT 
            var valid = sut.Validate(config);

            // ASSERT
            Assert.True(valid);
        }

        [Theory]
        [InlineData("\t")]
        [InlineData("  ")]
        public void GitHubAccessToken_must_not_be_whitespace(string accessToken)
        {
            // ARRANGE
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            config.Integrations.GitHub.AccessToken = accessToken;

            var sut = new ConfigurationValidator(m_Logger);

            // ACT 
            var valid = sut.Validate(config);

            // ASSERT
            Assert.False(valid);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void GitLabAccessToken_can_be_null_or_empty(string accessToken)
        {
            // ARRANGE
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            config.Integrations.GitLab.AccessToken = accessToken;

            var sut = new ConfigurationValidator(m_Logger);

            // ACT 
            var valid = sut.Validate(config);

            // ASSERT
            Assert.True(valid);
        }

        [Theory]
        [InlineData("\t")]
        [InlineData("  ")]
        public void GitLabAccessToken_must_not_be_whitespace(string accessToken)
        {
            // ARRANGE
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            config.Integrations.GitLab.AccessToken = accessToken;

            var sut = new ConfigurationValidator(m_Logger);

            // ACT 
            var valid = sut.Validate(config);

            // ASSERT
            Assert.False(valid);
        }
    }
}
