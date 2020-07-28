using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.ConventionalCommits;
using Grynwald.Utilities.Configuration;
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

        private static void AssertEntryType(ChangeLogConfiguration.EntryTypeConfiguration x, CommitType expectedCommitType, string expectedDisplayName)
        {
            // Compares a instance of EntryTypeConfiguration to the specified values.
            // This is a method instead of an inline lambda-expression, because a multi-line lambda
            // cannot be converted to an expression-tree.
            // However, expression trees are preferrable over lambdas because they make the actual assertion
            // visible in the test output.
            // (See https://twitter.com/bradwilson/status/1282374907670654976)

            Assert.NotNull(x.Type);
            Assert.NotEmpty(x.Type);
            Assert.Equal(expectedCommitType, new CommitType(x.Type!));
            Assert.Equal(expectedDisplayName, x.DisplayName);
        }


        /// <summary>
        /// Gets the assertions that must be true for the default configuration
        /// </summary>
        public static IEnumerable<object[]> DefaultConfigAssertions()
        {
            // Use Expression<Action<ChangeLogConfiguration>> instead of Action<ChangeLogConfiguration>
            // to make the assertion visible in the test-output
            // (see https://twitter.com/bradwilson/status/1282374907670654976)
            static object[] TestCase(Expression<Action<ChangeLogConfiguration>> assertion)
            {
                return new[] { assertion };
            }

            yield return TestCase(config => Assert.NotNull(config));

            yield return TestCase(config => Assert.NotNull(config.Scopes));
            yield return TestCase(config => Assert.Empty(config.Scopes));

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

            yield return TestCase(config => Assert.NotNull(config.Template));
            yield return TestCase(config => Assert.Equal(ChangeLogConfiguration.TemplateName.Default, config.Template.Name));

            yield return TestCase(config => Assert.NotNull(config.Template.Default));
            yield return TestCase(config => Assert.Equal(ChangeLogConfiguration.MarkdownPreset.Default, config.Template.Default.MarkdownPreset));

            yield return TestCase(config => Assert.NotNull(config.EntryTypes));
            yield return TestCase(config => Assert.Equal(2, config.EntryTypes.Length));
            yield return TestCase(config => Assert.Collection(config.EntryTypes,
                x => AssertEntryType(x, CommitType.Feature, "New Features"),
                x => AssertEntryType(x, CommitType.BugFix, "Bug Fixes")
            ));

            yield return TestCase(config => Assert.NotNull(config.Parser));
            yield return TestCase(config => Assert.Equal(ChangeLogConfiguration.ParserMode.Loose, config.Parser.Mode));
        }

        [Theory]
        [MemberData(nameof(DefaultConfigAssertions))]
        public void Default_configuration_is_valid(Expression<Action<ChangeLogConfiguration>> assertion)
        {
            var defaultConfig = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            assertion.Compile()(defaultConfig);
        }

        [Theory]
        [MemberData(nameof(DefaultConfigAssertions))]
        public void Empty_configuration_is_valid(Expression<Action<ChangeLogConfiguration>> assertion)
        {
            // ARRANGE           
            File.WriteAllText(m_ConfigurationFilePath, "{ }");

            // ACT
            var config = ChangeLogConfigurationLoader.GetConfiguration(m_ConfigurationFilePath);

            // ASSERT
            assertion.Compile()(config);
        }

        [Theory]
        [MemberData(nameof(DefaultConfigAssertions))]
        public void GetConfiguration_returns_default_configuration_if_config_file_does_not_exist(Expression<Action<ChangeLogConfiguration>> assertion)
        {
            var config = ChangeLogConfigurationLoader.GetConfiguration(m_ConfigurationFilePath);
            assertion.Compile()(config);
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
            var config = ChangeLogConfigurationLoader.GetConfiguration(m_ConfigurationFilePath);

            // ASSERT
            Assert.Equal(scopes.Length, config.Scopes.Length);
            for (var i = 0; i < scopes.Length; i++)
            {
                Assert.Equal(scopes[i].Name, config.Scopes[i].Name);
                Assert.Equal(scopes[i].DisplayName, config.Scopes[i].DisplayName);
            }
        }

        [Fact]
        public void TagPatterns_can_be_set_in_configuration_file()
        {
            // ARRANGE            
            var patterns = new[] { "pattern1", "pattern2" };

            PrepareConfiguration("tagpatterns", patterns);

            // ACT
            var config = ChangeLogConfigurationLoader.GetConfiguration(m_ConfigurationFilePath);

            // ASSERT
            Assert.Equal(patterns, config.TagPatterns);
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
            var config = ChangeLogConfigurationLoader.GetConfiguration(m_ConfigurationFilePath);

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
            var config = ChangeLogConfigurationLoader.GetConfiguration(m_ConfigurationFilePath);

            // ASSERT
            Assert.NotNull(config.Integrations);
            Assert.Equal(integrationProvider, config.Integrations.Provider);
        }

        [Theory]
        [MemberData(nameof(IntegrationProviders))]
        public void IntegrationProvider_can_be_set_through_environment_variables(ChangeLogConfiguration.IntegrationProvider integrationProvider)
        {
            // ARRANGE
            SetConfigEnvironmentVariable("integrations:provider", integrationProvider.ToString());

            // ACT 
            var config = ChangeLogConfigurationLoader.GetConfiguration(m_ConfigurationFilePath);

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
            var config = ChangeLogConfigurationLoader.GetConfiguration(m_ConfigurationFilePath);

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
            var config = ChangeLogConfigurationLoader.GetConfiguration(m_ConfigurationFilePath);

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
            var config = ChangeLogConfigurationLoader.GetConfiguration(m_ConfigurationFilePath, settingsObject);

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
            var config = ChangeLogConfigurationLoader.GetConfiguration(m_ConfigurationFilePath);

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
            var config = ChangeLogConfigurationLoader.GetConfiguration(m_ConfigurationFilePath);

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
            var config = ChangeLogConfigurationLoader.GetConfiguration(m_ConfigurationFilePath, settingsObject);

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
            var config = ChangeLogConfigurationLoader.GetConfiguration(m_ConfigurationFilePath);

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
            var config = ChangeLogConfigurationLoader.GetConfiguration(m_ConfigurationFilePath);

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
            var config = ChangeLogConfigurationLoader.GetConfiguration(m_ConfigurationFilePath, settingsObject);

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
            var config = ChangeLogConfigurationLoader.GetConfiguration(m_ConfigurationFilePath);

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
            var config = ChangeLogConfigurationLoader.GetConfiguration(m_ConfigurationFilePath);

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
            var config = ChangeLogConfigurationLoader.GetConfiguration(m_ConfigurationFilePath, settingsObject);

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
            var config = ChangeLogConfigurationLoader.GetConfiguration(m_ConfigurationFilePath);

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
            var config = ChangeLogConfigurationLoader.GetConfiguration(m_ConfigurationFilePath, settingsObject);

            // ASSERT
            Assert.NotNull(config.CurrentVersion);
            Assert.Equal("7.8.9", config.CurrentVersion);
        }

        [Fact]
        public void OutputPath_can_be_set_in_the_configuration_file()
        {
            // ARRANGE
            PrepareConfiguration("outputPath", "outputPath1");

            // ACT 
            var config = ChangeLogConfigurationLoader.GetConfiguration(m_ConfigurationFilePath);

            // ASSERT
            Assert.Equal("outputPath1", config.OutputPath);
        }

        [Fact]
        public void OutputPath_can_be_set_through_environment_variables()
        {
            // ARRANGE
            PrepareConfiguration("outputPath", "outputPath1");
            SetConfigEnvironmentVariable("outputPath", "outputPath2");

            // ACT 
            var config = ChangeLogConfigurationLoader.GetConfiguration(m_ConfigurationFilePath);

            // ASSERT
            // environment variables override configuration file
            Assert.Equal("outputPath2", config.OutputPath);
        }

        private class TestSettingsClass7
        {
            [ConfigurationValue("changelog:outputPath")]
            public string? OutputPath { get; set; }
        }

        [Fact]
        public void OutputPath_from_settings_object_overrides_output_path()
        {
            // ARRANGE
            PrepareConfiguration("outputPath", "outputPath1");
            SetConfigEnvironmentVariable("outputPath", "outputPath2");

            var settingsObject = new TestSettingsClass7() { OutputPath = "outputPath3" };

            // ACT
            var config = ChangeLogConfigurationLoader.GetConfiguration(m_ConfigurationFilePath, settingsObject);

            // ASSERT
            // settings objects overrides both configuration file and environment variables
            Assert.Equal("outputPath3", config.OutputPath);
        }


        [Theory]
        [EnumData]
        public void Template_can_be_set_in_the_configuration_file(ChangeLogConfiguration.TemplateName templateName)
        {
            // ARRANGE
            PrepareConfiguration("template:name", templateName.ToString());

            // ACT 
            var config = ChangeLogConfigurationLoader.GetConfiguration(m_ConfigurationFilePath);

            // ASSERT
            Assert.NotNull(config.Template);
            Assert.Equal(templateName, config.Template.Name);
        }

        [Theory]
        [EnumData]
        public void Template_can_be_set_through_environment_variables(ChangeLogConfiguration.TemplateName templateName)
        {
            // ARRANGE
            SetConfigEnvironmentVariable("template:name", templateName.ToString());

            // ACT 
            var config = ChangeLogConfigurationLoader.GetConfiguration(m_ConfigurationFilePath);

            // ASSERT
            Assert.NotNull(config.Template);
            Assert.Equal(templateName, config.Template.Name);
        }

        private class TestSettingsClass8
        {
            [ConfigurationValue("changelog:template:name")]
            public ChangeLogConfiguration.TemplateName TemplateName { get; set; }
        }

        [Theory]
        [EnumData]
        public void Template_can_be_set_through_settings_object(ChangeLogConfiguration.TemplateName templateName)
        {
            // ARRANGE
            var settingsObject = new TestSettingsClass8() { TemplateName = templateName };

            // ACT 
            var config = ChangeLogConfigurationLoader.GetConfiguration(m_ConfigurationFilePath, settingsObject);

            // ASSERT
            Assert.NotNull(config.Template);
            Assert.Equal(templateName, config.Template.Name);
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
        public void Markdown_preset_for_default_template_can_be_set_in_configuration_file(ChangeLogConfiguration.MarkdownPreset preset, string configurationValue)
        {
            // ARRANGE
            PrepareConfiguration("template:default:markdownpreset", configurationValue);

            // ACT 
            var config = ChangeLogConfigurationLoader.GetConfiguration(m_ConfigurationFilePath);

            // ASSERT
            Assert.NotNull(config.Template.Default);
            Assert.Equal(preset, config.Template.Default.MarkdownPreset);
        }

        [Theory]
        [MemberData(nameof(MarkdownPresets))]
        public void Markdown_preset_for_default_template_can_be_set_through_environment_variables(ChangeLogConfiguration.MarkdownPreset preset, string configurationValue)
        {
            // ARRANGE
            SetConfigEnvironmentVariable("template:default:markdownpreset", configurationValue);

            // ACT 
            var config = ChangeLogConfigurationLoader.GetConfiguration(m_ConfigurationFilePath);

            // ASSERT
            Assert.NotNull(config.Template.Default);
            Assert.Equal(preset, config.Template.Default.MarkdownPreset);
        }


        [Fact]
        public void Configuration_from_environment_variables_can_override_markown_preset_for_default_template()
        {
            // ARRANGE
            PrepareConfiguration("template:default:markdownpreset", "default");
            SetConfigEnvironmentVariable("template:default:markdownpreset", "mkdocs");

            // ACT 
            var config = ChangeLogConfigurationLoader.GetConfiguration(m_ConfigurationFilePath);

            // ASSERT
            Assert.NotNull(config.Template.Default);
            Assert.Equal(ChangeLogConfiguration.MarkdownPreset.MkDocs, config.Template.Default.MarkdownPreset);
        }

        private class TestSettingsClass9
        {
            [ConfigurationValue("changelog:template:default:markdownpreset")]
            public string? MarkdownPreset { get; set; }
        }

        [Fact]
        public void Configuration_from_settings_object_can_override_markown_preset_for_default_template()
        {
            // ARRANGE
            PrepareConfiguration("template:default:markdownpreset", "default");
            var settingsObject = new TestSettingsClass9() { MarkdownPreset = "MkDocs" };

            // ACT 
            var config = ChangeLogConfigurationLoader.GetConfiguration(m_ConfigurationFilePath, settingsObject);

            // ASSERT
            Assert.NotNull(config.Template.Default);
            Assert.Equal(ChangeLogConfiguration.MarkdownPreset.MkDocs, config.Template.Default.MarkdownPreset);
        }

        [Fact]
        public void EntryTypes_can_be_set_in_the_configuration_file()
        {
            // ARRANGE            
            var entryTypes = new[]
            {
                new ChangeLogConfiguration.EntryTypeConfiguration() { Type = "docs", DisplayName = "Documentation Updates" },
                new ChangeLogConfiguration.EntryTypeConfiguration() { Type = "bugfix", DisplayName = "Bug Fixes" }
            };

            PrepareConfiguration("entryTypes", entryTypes);

            // ACT
            var config = ChangeLogConfigurationLoader.GetConfiguration(m_ConfigurationFilePath);

            // ASSERT
            var assertions = entryTypes
                .Select<ChangeLogConfiguration.EntryTypeConfiguration, Action<ChangeLogConfiguration.EntryTypeConfiguration>>(
                    expected => actual =>
                    {
                        Assert.Equal(expected.Type, actual.Type);
                        Assert.Equal(expected.DisplayName, actual.DisplayName);
                    })
                .ToArray();

            Assert.Equal(entryTypes.Length, config.EntryTypes.Length);
            Assert.Collection(config.EntryTypes, assertions);
        }

        [Theory]
        [EnumData]
        public void ParserMode_can_be_set_in_configuration_file(ChangeLogConfiguration.ParserMode value)
        {
            // ARRANGE
            PrepareConfiguration("parser:mode", value);

            // ACT 
            var config = ChangeLogConfigurationLoader.GetConfiguration(m_ConfigurationFilePath);

            // ASSERT
            Assert.NotNull(config.Parser);
            Assert.Equal(value, config.Parser.Mode);
        }
    }
}
