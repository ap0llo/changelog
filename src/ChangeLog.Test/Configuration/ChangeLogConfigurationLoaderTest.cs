using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Grynwald.ChangeLog.Configuration;
using Grynwald.Utilities.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Grynwald.ChangeLog.Test.Configuration
{
    public class ChangeLogConfigurationLoaderTest : IDisposable
    {
        private readonly TemporaryDirectory m_ConfigurationDirectory = new TemporaryDirectory();
        private readonly string m_ConfigurationFilePath;

        public ChangeLogConfigurationLoaderTest()
        {
            m_ConfigurationFilePath = Path.Combine(m_ConfigurationDirectory, "changelog.settings.json");

            // clear environment variables (might be set by previous test runs)
            var envVars = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Process);
            foreach (var key in envVars.Keys.Cast<string>().Where(x => x.StartsWith("CHANGELOG__")))
            {
                Environment.SetEnvironmentVariable(key, null, EnvironmentVariableTarget.Process);
            }
        }

        public void Dispose() => m_ConfigurationDirectory.Dispose();


        private void SetConfigEnvironmentVariable(string configKey, string value)
        {
            var variableName = "CHANGELOG__" + configKey.Replace(":", "__");
            Environment.SetEnvironmentVariable(variableName, value, EnvironmentVariableTarget.Process);
        }

        private void PrepareConfiguration(string key, object value)
        {
            var configRoot = new JObject();

            var currentConfigObject = new JObject();
            configRoot.Add(new JProperty("changelog", currentConfigObject));

            var keySegments = key.Split(":");
            for (var i = 0; i < keySegments.Length; i++)
            {
                // last fragment => add value
                if (i == keySegments.Length - 1)
                {
                    if (value.GetType().IsArray)
                    {
                        value = JArray.FromObject(value);
                    }

                    currentConfigObject.Add(new JProperty(keySegments[i], value));

                }
                // create child configuration object
                else
                {
                    var newConfigObject = new JObject();
                    currentConfigObject.Add(new JProperty(keySegments[i], newConfigObject));
                    currentConfigObject = newConfigObject;
                }
            }

            var json = configRoot.ToString(Formatting.Indented);
            File.WriteAllText(m_ConfigurationFilePath, json);
        }


        /// <summary>
        /// Gets the assertions that must be true for the default configuration
        /// </summary>
        public static IEnumerable<object[]> DefaultConfigAssertions()
        {
            static object[] TestCase(Action<ChangeLogConfiguration> assertion)
            {
                return new[] { assertion };
            }

            yield return TestCase(config => Assert.NotNull(config));

            yield return TestCase(config => Assert.NotNull(config.Scopes));
            yield return TestCase(config => Assert.Empty(config.Scopes));

            yield return TestCase(config => Assert.NotNull(config.Markdown));
            yield return TestCase(config => Assert.Equal(ChangeLogConfiguration.MarkdownPreset.Default, config.Markdown.Preset));

            yield return TestCase(config => Assert.NotNull(config.TagPatterns));
            yield return TestCase(config => Assert.Equal(new[] { "^(?<version>\\d+\\.\\d+\\.\\d+.*)", "^v(?<version>\\d+\\.\\d+\\.\\d+.*)" }, config.TagPatterns));

            yield return TestCase(config => Assert.NotNull(config.OutputPath));
            yield return TestCase(config => Assert.NotEmpty(config.OutputPath));

            yield return TestCase(config => Assert.Null(config.RepositoryPath));    // repository path must be provided through command line parameters

            yield return TestCase(config => Assert.NotNull(config.Footers));
            yield return TestCase(config => Assert.Empty(config.Footers));

            yield return TestCase(config => Assert.NotNull(config.Integrations));
            yield return TestCase(config => Assert.Equal(ChangeLogConfiguration.IntegrationProvider.None, config.Integrations.Provider));

            yield return TestCase(config => Assert.NotNull(config.Integrations.GitHub));
            yield return TestCase(config => Assert.NotNull(config.Integrations.GitHub.AccessToken));
            yield return TestCase(config => Assert.Empty(config.Integrations.GitHub.AccessToken));

            yield return TestCase(config => Assert.NotNull(config.Integrations.GitLab));
            yield return TestCase(config => Assert.NotNull(config.Integrations.GitLab.AccessToken));
            yield return TestCase(config => Assert.Empty(config.Integrations.GitLab.AccessToken));

            yield return TestCase(config => Assert.NotNull(config.VersionRange));
            yield return TestCase(config => Assert.Empty(config.VersionRange));

            yield return TestCase(config => Assert.NotNull(config.CurrentVersion));
            yield return TestCase(config => Assert.Empty(config.CurrentVersion));
        }

        [Theory]
        [MemberData(nameof(DefaultConfigAssertions))]
        public void Default_configuration_is_valid(Action<ChangeLogConfiguration> assertion)
        {
            var defaultConfig = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            assertion(defaultConfig);
        }

        [Theory]
        [MemberData(nameof(DefaultConfigAssertions))]
        public void Empty_configuration_is_valid(Action<ChangeLogConfiguration> assertion)
        {
            // ARRANGE           
            File.WriteAllText(m_ConfigurationFilePath, "{ }");

            // ACT
            var config = ChangeLogConfigurationLoader.GetConfiguation(m_ConfigurationFilePath);

            // ASSERT
            assertion(config);
        }

        [Theory]
        [MemberData(nameof(DefaultConfigAssertions))]
        public void GetConfiguration_returns_default_configuration_if_config_file_does_not_exist(Action<ChangeLogConfiguration> assertion)
        {
            var config = ChangeLogConfigurationLoader.GetConfiguation(m_ConfigurationFilePath);
            assertion(config);
        }

        [Fact]
        public void Scopes_can_be_set_in_configuration_file()
        {
            // ARRANGE            
            var scopes = new[]
            {
                new ChangeLogConfiguration.ScopeConfiguration() { Name = "scope1", DisplayName = "DisplayName 1" },
                new ChangeLogConfiguration.ScopeConfiguration() { Name = "scope2", DisplayName = "DisplayName 2" }
            };

            PrepareConfiguration("scopes", scopes);

            // ACT
            var config = ChangeLogConfigurationLoader.GetConfiguation(m_ConfigurationFilePath);

            // ASSERT
            Assert.Equal(scopes.Length, config.Scopes.Length);
            for (var i = 0; i < scopes.Length; i++)
            {
                Assert.Equal(scopes[i].Name, config.Scopes[i].Name);
                Assert.Equal(scopes[i].DisplayName, config.Scopes[i].DisplayName);
            }
        }

        public static IEnumerable<object[]> MarkdownPresets()
        {
            foreach (var value in Enum.GetValues(typeof(ChangeLogConfiguration.MarkdownPreset)))
            {
                yield return new object[] { value!, value!.ToString()! };
                yield return new object[] { value!, value!.ToString()!.ToLower() };
            }
        }

        [Theory]
        [MemberData(nameof(MarkdownPresets))]
        public void Markdown_preset_can_be_set_in_configuration_file(ChangeLogConfiguration.MarkdownPreset preset, string configurationValue)
        {
            // ARRANGE
            PrepareConfiguration("markdown:preset", configurationValue);

            // ACT 
            var config = ChangeLogConfigurationLoader.GetConfiguation(m_ConfigurationFilePath);

            // ASSERT
            Assert.NotNull(config.Markdown);
            Assert.Equal(preset, config.Markdown.Preset);
        }

        [Fact]
        public void TagPatterns_can_be_set_in_configuration_file()
        {
            // ARRANGE            
            var patterns = new[] { "pattern1", "pattern2" };

            PrepareConfiguration("tagpatterns", patterns);

            // ACT
            var config = ChangeLogConfigurationLoader.GetConfiguation(m_ConfigurationFilePath);

            // ASSERT
            Assert.Equal(patterns, config.TagPatterns);
        }

        private class TestSettingsClass1
        {
            [ConfigurationValue("changelog:markdown:preset")]
            public string? MarkdownPreset { get; set; }
        }

        [Fact]
        public void Configuration_from_settings_object_can_override_markown_preset()
        {
            // ARRANGE
            PrepareConfiguration("markdown:preset", "default");
            var settingsObject = new TestSettingsClass1() { MarkdownPreset = "MkDocs" };

            // ACT 
            var config = ChangeLogConfigurationLoader.GetConfiguation(m_ConfigurationFilePath, settingsObject);

            // ASSERT
            Assert.NotNull(config.Markdown);
            Assert.Equal(ChangeLogConfiguration.MarkdownPreset.MkDocs, config.Markdown.Preset);
        }

        [Fact]
        public void Configuration_from_environment_variables_can_override_markown_preset()
        {
            // ARRANGE
            PrepareConfiguration("markdown:preset", "default");
            SetConfigEnvironmentVariable("markdown:preset", "mkdocs");

            // ACT 
            var config = ChangeLogConfigurationLoader.GetConfiguation(m_ConfigurationFilePath);

            // ASSERT
            Assert.NotNull(config.Markdown);
            Assert.Equal(ChangeLogConfiguration.MarkdownPreset.MkDocs, config.Markdown.Preset);
        }

        [Fact]
        public void Footers_can_be_set_in_configuration_file()
        {
            // ARRANGE            
            var footers = new[]
            {
                new ChangeLogConfiguration.FooterConfiguration() { Name = "footer1", DisplayName = "DisplayName 1" },
                new ChangeLogConfiguration.FooterConfiguration() { Name = "footer2", DisplayName = "DisplayName 2" }
            };

            PrepareConfiguration("footers", footers);

            // ACT
            var config = ChangeLogConfigurationLoader.GetConfiguation(m_ConfigurationFilePath);

            // ASSERT
            Assert.Equal(footers.Length, config.Footers.Length);
            for (var i = 0; i < footers.Length; i++)
            {
                Assert.Equal(footers[i].Name, config.Footers[i].Name);
                Assert.Equal(footers[i].DisplayName, config.Footers[i].DisplayName);
            }
        }

        public static IEnumerable<object?[]> IntegrationProviders()
        {
            foreach (var value in Enum.GetValues(typeof(ChangeLogConfiguration.IntegrationProvider)))
            {
                yield return new object?[] { value };
            }
        }

        [Theory]
        [MemberData(nameof(IntegrationProviders))]
        public void IntegrationProvider_can_be_set_in_configuration_file(ChangeLogConfiguration.IntegrationProvider integrationProvider)
        {
            // ARRANGE
            PrepareConfiguration("integrations:provider", integrationProvider.ToString());

            // ACT 
            var config = ChangeLogConfigurationLoader.GetConfiguation(m_ConfigurationFilePath);

            // ASSERT
            Assert.NotNull(config.Integrations);
            Assert.Equal(integrationProvider, config.Integrations.Provider);
        }

        [Theory]
        [InlineData("some-access-token")]
        public void GitHub_access_token_can_be_set_in_configuration_file(string accessToken)
        {
            // ARRANGE
            PrepareConfiguration("integrations:github:accesstoken", accessToken);

            // ACT 
            var config = ChangeLogConfigurationLoader.GetConfiguation(m_ConfigurationFilePath);

            // ASSERT
            Assert.NotNull(config.Integrations.GitHub);
            Assert.Equal(accessToken, config.Integrations.GitHub.AccessToken);
        }

        [Theory]
        [InlineData("some-access-token")]
        public void GitHub_access_token_can_be_set_through_environment_variables(string accessToken)
        {
            // ARRANGE
            SetConfigEnvironmentVariable("integrations:github:accesstoken", accessToken);

            // ACT 
            var config = ChangeLogConfigurationLoader.GetConfiguation(m_ConfigurationFilePath);

            // ASSERT
            Assert.NotNull(config.Integrations.GitHub);
            Assert.Equal(accessToken, config.Integrations.GitHub.AccessToken);
        }

        private class TestSettingsClass2
        {
            [ConfigurationValue("changelog:integrations:github:accesstoken")]
            public string? GitHubAccessToken { get; set; }
        }

        [Fact]
        public void Configuration_from_settings_object_can_override_GitHub_access_token()
        {
            // ARRANGE
            PrepareConfiguration("integrations:github:accesstoken", "some-access-token");
            var settingsObject = new TestSettingsClass2() { GitHubAccessToken = "some-other-access-token" };

            // ACT 
            var config = ChangeLogConfigurationLoader.GetConfiguation(m_ConfigurationFilePath, settingsObject);

            // ASSERT
            Assert.NotNull(config.Integrations);
            Assert.NotNull(config.Integrations.GitHub);
            Assert.Equal("some-other-access-token", config.Integrations.GitHub.AccessToken);
        }

        [Theory]
        [InlineData("some-access-token")]
        public void GitLab_access_token_can_be_set_in_configuration_file(string accessToken)
        {
            // ARRANGE
            PrepareConfiguration("integrations:gitlab:accesstoken", accessToken);

            // ACT 
            var config = ChangeLogConfigurationLoader.GetConfiguation(m_ConfigurationFilePath);

            // ASSERT
            Assert.NotNull(config.Integrations.GitLab);
            Assert.Equal(accessToken, config.Integrations.GitLab.AccessToken);
        }

        [Theory]
        [InlineData("some-access-token")]
        public void GitLab_access_token_can_be_set_through_environment_variables(string accessToken)
        {
            // ARRANGE
            SetConfigEnvironmentVariable("integrations:gitlab:accesstoken", accessToken);

            // ACT 
            var config = ChangeLogConfigurationLoader.GetConfiguation(m_ConfigurationFilePath);

            // ASSERT
            Assert.NotNull(config.Integrations.GitLab);
            Assert.Equal(accessToken, config.Integrations.GitLab.AccessToken);
        }

        private class TestSettingsClass3
        {
            [ConfigurationValue("changelog:integrations:gitlab:accesstoken")]
            public string? GitLabAccessToken { get; set; }
        }

        [Fact]
        public void Configuration_from_settings_object_can_override_GitLab_access_token()
        {
            // ARRANGE
            PrepareConfiguration("integrations:gitlab:accesstoken", "some-access-token");
            var settingsObject = new TestSettingsClass3() { GitLabAccessToken = "some-other-access-token" };

            // ACT 
            var config = ChangeLogConfigurationLoader.GetConfiguation(m_ConfigurationFilePath, settingsObject);

            // ASSERT
            Assert.NotNull(config.Integrations);
            Assert.NotNull(config.Integrations.GitLab);
            Assert.Equal("some-other-access-token", config.Integrations.GitLab.AccessToken);
        }

        [Theory]
        [InlineData("[1.2.3,)")]
        public void Version_range_can_be_set_in_configuration_file(string versionRange)
        {
            // ARRANGE
            PrepareConfiguration("versionRange", versionRange);

            // ACT 
            var config = ChangeLogConfigurationLoader.GetConfiguation(m_ConfigurationFilePath);

            // ASSERT
            Assert.NotNull(config.VersionRange);
            Assert.Equal(versionRange, config.VersionRange);
        }

        [Theory]
        [InlineData("[1.2.3,)")]
        public void Version_range_can_be_set_through_environment_variables(string versionRange)
        {
            // ARRANGE
            SetConfigEnvironmentVariable("versionRange", versionRange);

            // ACT 
            var config = ChangeLogConfigurationLoader.GetConfiguation(m_ConfigurationFilePath);

            // ASSERT
            Assert.NotNull(config.VersionRange);
            Assert.Equal(versionRange, config.VersionRange);
        }


        private class TestSettingsClass4
        {
            [ConfigurationValue("changelog:versionrange")]
            public string? VersionRange { get; set; }
        }

        [Fact]
        public void Configuration_from_settings_object_can_override_version_range()
        {
            // ARRANGE
            PrepareConfiguration("versionRange", "[1.2.3]");
            var settingsObject = new TestSettingsClass4() { VersionRange = "[4.5.6]" };

            // ACT 
            var config = ChangeLogConfigurationLoader.GetConfiguation(m_ConfigurationFilePath, settingsObject);

            // ASSERT
            Assert.NotNull(config.VersionRange);
            Assert.Equal("[4.5.6]", config.VersionRange);
        }

        [Theory]
        [InlineData("1.2.3")]
        public void Current_version_can_be_set_in_the_configuration_file(string currentVersion)
        {
            // ARRANGE
            PrepareConfiguration("currentVersion", currentVersion);

            // ACT 
            var config = ChangeLogConfigurationLoader.GetConfiguation(m_ConfigurationFilePath);

            // ASSERT
            Assert.NotNull(config.CurrentVersion);
            Assert.Equal(currentVersion, config.CurrentVersion);
        }

        [Theory]
        [InlineData("1.2.3")]
        public void Current_version_can_be_set_in_through_environment_variables(string currentVersion)
        {
            // ARRANGE
            SetConfigEnvironmentVariable("currentVersion", currentVersion);

            // ACT 
            var config = ChangeLogConfigurationLoader.GetConfiguation(m_ConfigurationFilePath);

            // ASSERT
            Assert.NotNull(config.CurrentVersion);
            Assert.Equal(currentVersion, config.CurrentVersion);
        }


        private class TestSettingsClass5
        {
            [ConfigurationValue("changelog:currentVersion")]
            public string? CurrentVersion { get; set; }
        }

        [Fact]
        public void Configuration_from_settings_object_can_override_current_version()
        {
            // ARRANGE
            PrepareConfiguration("currentVersion", "1.2.3");
            var settingsObject = new TestSettingsClass5() { CurrentVersion = "4.5.6" };

            // ACT 
            var config = ChangeLogConfigurationLoader.GetConfiguation(m_ConfigurationFilePath, settingsObject);

            // ASSERT
            Assert.NotNull(config.CurrentVersion);
            Assert.Equal("4.5.6", config.CurrentVersion);
        }

        [Fact]
        public void Configuration_from_environment_variables_overrides_settings_from_config_file()
        {
            // ARRANGE
            PrepareConfiguration("currentVersion", "1.2.3");
            SetConfigEnvironmentVariable("currentVersion", "4.5.6");

            // ACT
            var config = ChangeLogConfigurationLoader.GetConfiguation(m_ConfigurationFilePath);

            // ASSERT
            Assert.NotNull(config.CurrentVersion);
            Assert.Equal("4.5.6", config.CurrentVersion);
        }

        private class TestSettingsClass6
        {
            [ConfigurationValue("changelog:currentVersion")]
            public string? CurrentVersion { get; set; }
        }

        [Fact]
        public void Configuration_from_settings_object_overrides_settings_from_config_file_and_environment_variables()
        {
            // ARRANGE
            PrepareConfiguration("currentVersion", "1.2.3");
            SetConfigEnvironmentVariable("currentVersion", "4.5.6");
            var settingsObject = new TestSettingsClass6() { CurrentVersion = "7.8.9" };

            // ACT
            var config = ChangeLogConfigurationLoader.GetConfiguation(m_ConfigurationFilePath, settingsObject);

            // ASSERT
            Assert.NotNull(config.CurrentVersion);
            Assert.Equal("7.8.9", config.CurrentVersion);
        }
    }
}
