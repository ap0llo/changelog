using System;
using System.Collections.Generic;
using System.IO;
using ChangeLogCreator.Configuration;
using Grynwald.Utilities.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace ChangeLogCreator.Test.Configuration
{
    public class ChangeLogConfigurationLoaderTest : IDisposable
    {
        private readonly TemporaryDirectory m_ConfigurationDirectory = new TemporaryDirectory();
        private readonly string m_ConfigurationFilePath;

        public ChangeLogConfigurationLoaderTest()
        {
            m_ConfigurationFilePath = Path.Combine(m_ConfigurationDirectory, "changelog.settings.json");
        }

        public void Dispose() => m_ConfigurationDirectory.Dispose();


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
            var config = ChangeLogConfigurationLoader.GetConfiguation(m_ConfigurationDirectory);

            // ASSERT
            assertion(config);
        }

        [Theory]
        [MemberData(nameof(DefaultConfigAssertions))]
        public void GetConfiguration_returns_default_configuration_if_config_file_does_not_exist(Action<ChangeLogConfiguration> assertion)
        {
            var config = ChangeLogConfigurationLoader.GetConfiguation(m_ConfigurationDirectory);
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
            var config = ChangeLogConfigurationLoader.GetConfiguation(m_ConfigurationDirectory);

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
            foreach(var value in Enum.GetValues(typeof(ChangeLogConfiguration.MarkdownPreset)))
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
            var config = ChangeLogConfigurationLoader.GetConfiguation(m_ConfigurationDirectory);

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
            var config = ChangeLogConfigurationLoader.GetConfiguation(m_ConfigurationDirectory);

            // ASSERT
            Assert.Equal(patterns, config.TagPatterns);            
        }

        private class TestSettingsClass
        {
            [ConfigurationValue("changelog:markdown:preset")]
            public string? MarkdownPreset { get; set; }
        }

        [Fact]
        public void Configuration_from_settings_object_overrides_both_default_settings_and_settings_file()
        {
            // ARRANGE
            PrepareConfiguration("markdown:preset", "default");
            var settingsObject = new TestSettingsClass() { MarkdownPreset = "MkDocs" };

            // ACT 
            var config = ChangeLogConfigurationLoader.GetConfiguation(m_ConfigurationDirectory, settingsObject);

            // ASSERT
            Assert.NotNull(config.Markdown);
            Assert.Equal(ChangeLogConfiguration.MarkdownPreset.MkDocs, config.Markdown.Preset);
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
    }
}
