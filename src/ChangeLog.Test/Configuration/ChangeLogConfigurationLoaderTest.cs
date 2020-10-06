using System;
using System.Collections;
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
    /// <summary>
    /// Tests for <see cref="ChangeLogConfigurationLoader"/>.
    /// </summary>
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


        private void SetConfigEnvironmentVariable(string configKey, string? value)
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
                    if (value is null)
                    {
                        currentConfigObject.Add(new JProperty(keySegments[i], (object?)null));
                    }
                    else if (value.GetType().IsArray)
                    {
                        currentConfigObject.Add(new JProperty(keySegments[i], JArray.FromObject(value)));
                    }
                    else if (value is IDictionary)
                    {
                        var jsonValue = JsonConvert.SerializeObject(value);
                        currentConfigObject.Add(new JProperty(keySegments[i], JObject.Parse(jsonValue)));
                    }
                    else
                    {
                        currentConfigObject.Add(new JProperty(keySegments[i], value));
                    }

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

        private static void AssertEntryType(KeyValuePair<string, ChangeLogConfiguration.EntryTypeConfiguration> x, CommitType expectedCommitType, string expectedDisplayName, int expectedOrder)
        {
            // Compares a instance of EntryTypeConfiguration to the specified values.
            // This is a method instead of an inline lambda-expression, because a multi-line lambda
            // cannot be converted to an expression-tree.
            // However, expression trees are preferrable over lambdas because they make the actual assertion
            // visible in the test output.
            // (See https://twitter.com/bradwilson/status/1282374907670654976)

            Assert.NotNull(x.Key);
            Assert.NotEmpty(x.Key);
            Assert.Equal(expectedCommitType, new CommitType(x.Key));
            Assert.Equal(expectedDisplayName, x.Value.DisplayName);
            Assert.Equal(expectedOrder, x.Value.Order);
        }

        private static void AssertFilterExpression(ChangeLogConfiguration.FilterExpressionConfiguration x, string type, string scope)
        {
            Assert.NotNull(x);
            Assert.Equal(type, x.Type);
            Assert.Equal(scope, x.Scope);
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

            //
            // Scope settings
            //
            yield return TestCase(config => Assert.NotNull(config.Scopes));
            yield return TestCase(config => Assert.Empty(config.Scopes));

            //
            // Tag Patterns setting
            //
            yield return TestCase(config => Assert.NotNull(config.TagPatterns));
            yield return TestCase(config => Assert.Equal(new[] { "^(?<version>\\d+\\.\\d+\\.\\d+.*)", "^v(?<version>\\d+\\.\\d+\\.\\d+.*)" }, config.TagPatterns));

            //
            // Output path setting
            //
            yield return TestCase(config => Assert.NotNull(config.OutputPath));
            yield return TestCase(config => Assert.NotEmpty(config.OutputPath));

            //
            // Repository Path setting
            //
            yield return TestCase(config => Assert.Null(config.RepositoryPath));    // repository path must be provided through command line parameters

            //
            // Footer settings
            //
            yield return TestCase(config => Assert.NotNull(config.Footers));
            yield return TestCase(config => Assert.NotEmpty(config.Footers));
            yield return TestCase(config => Assert.Equal(7, config.Footers.Count));
            yield return TestCase(config => Assert.Contains("see-also", config.Footers.Keys));
            yield return TestCase(config => Assert.Contains("closes", config.Footers.Keys));
            yield return TestCase(config => Assert.Contains("fixes", config.Footers.Keys));
            yield return TestCase(config => Assert.Contains("co-authored-by", config.Footers.Keys));
            yield return TestCase(config => Assert.Contains("reviewed-by", config.Footers.Keys));
            yield return TestCase(config => Assert.Contains("pull-request", config.Footers.Keys));
            yield return TestCase(config => Assert.Contains("merge-request", config.Footers.Keys));

            //
            // Integration Provider setting
            //
            yield return TestCase(config => Assert.NotNull(config.Integrations));
            yield return TestCase(config => Assert.Equal(ChangeLogConfiguration.IntegrationProvider.None, config.Integrations.Provider));

            //
            // GitHub Integration settings
            //
            yield return TestCase(config => Assert.NotNull(config.Integrations.GitHub));

            yield return TestCase(config => Assert.NotNull(config.Integrations.GitHub.AccessToken));
            yield return TestCase(config => Assert.Empty(config.Integrations.GitHub.AccessToken));

            yield return TestCase(config => Assert.Equal("origin", config.Integrations.GitHub.RemoteName));

            yield return TestCase(config => Assert.NotNull(config.Integrations.GitHub.Host));
            yield return TestCase(config => Assert.Empty(config.Integrations.GitHub.Host));

            yield return TestCase(config => Assert.NotNull(config.Integrations.GitHub.Owner));
            yield return TestCase(config => Assert.Empty(config.Integrations.GitHub.Owner));

            yield return TestCase(config => Assert.NotNull(config.Integrations.GitHub.Repository));
            yield return TestCase(config => Assert.Empty(config.Integrations.GitHub.Repository));

            //
            // GitLab Integration settings
            //
            yield return TestCase(config => Assert.NotNull(config.Integrations.GitLab));

            yield return TestCase(config => Assert.NotNull(config.Integrations.GitLab.AccessToken));
            yield return TestCase(config => Assert.Empty(config.Integrations.GitLab.AccessToken));

            yield return TestCase(config => Assert.Equal("origin", config.Integrations.GitLab.RemoteName));

            yield return TestCase(config => Assert.NotNull(config.Integrations.GitLab.Host));
            yield return TestCase(config => Assert.Empty(config.Integrations.GitLab.Host));

            yield return TestCase(config => Assert.NotNull(config.Integrations.GitLab.Namespace));
            yield return TestCase(config => Assert.Empty(config.Integrations.GitLab.Namespace));

            yield return TestCase(config => Assert.NotNull(config.Integrations.GitLab.Project));
            yield return TestCase(config => Assert.Empty(config.Integrations.GitLab.Project));

            //
            // Version Range setting
            //
            yield return TestCase(config => Assert.NotNull(config.VersionRange));
            yield return TestCase(config => Assert.Empty(config.VersionRange));

            //
            // Current Version settting
            //
            yield return TestCase(config => Assert.NotNull(config.CurrentVersion));
            yield return TestCase(config => Assert.Empty(config.CurrentVersion));

            //
            // Template settings
            //
            yield return TestCase(config => Assert.NotNull(config.Template));
            yield return TestCase(config => Assert.Equal(ChangeLogConfiguration.TemplateName.Default, config.Template.Name));

            //
            // Default Template settings
            // 
            yield return TestCase(config => Assert.NotNull(config.Template.Default));
            yield return TestCase(config => Assert.Equal(ChangeLogConfiguration.MarkdownPreset.Default, config.Template.Default.MarkdownPreset));

            //
            // Entry Types settings
            //
            yield return TestCase(config => Assert.NotNull(config.EntryTypes));
            yield return TestCase(config => Assert.Equal(2, config.EntryTypes.Count));
            yield return TestCase(config => Assert.Collection(config.EntryTypes,
                x => AssertEntryType(x, CommitType.Feature, "New Features", 10),
                x => AssertEntryType(x, CommitType.BugFix, "Bug Fixes", 20)
            ));

            //
            // Parser settings
            //
            yield return TestCase(config => Assert.NotNull(config.Parser));
            yield return TestCase(config => Assert.Equal(ChangeLogConfiguration.ParserMode.Loose, config.Parser.Mode));

            //
            // Filter settings
            //
            yield return TestCase(config => Assert.NotNull(config.Filter));

            yield return TestCase(config => Assert.NotNull(config.Filter.Include));
            yield return TestCase(config => Assert.Collection(config.Filter.Include,
                x => AssertFilterExpression(x, "feat", "*"),
                x => AssertFilterExpression(x, "fix", "*")
            ));

            yield return TestCase(config => Assert.NotNull(config.Filter.Exclude));
            yield return TestCase(config => Assert.Empty(config.Filter.Exclude));
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
        public void Value_from_environment_variables_overrides_value_from_config_file()
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

        private class TestSettingsClass
        {
            [ConfigurationValue("changelog:currentVersion")]
            public string? CurrentVersion { get; set; }
        }

        [Fact]
        public void Value_from_settings_object_overrides_value_from_config_file()
        {
            // ARRANGE
            PrepareConfiguration("currentVersion", "1.2.3");

            var settingsObject = new TestSettingsClass()
            {
                CurrentVersion = "4.5.6"
            };

            // ACT
            var config = ChangeLogConfigurationLoader.GetConfiguration(m_ConfigurationFilePath, settingsObject);

            // ASSERT
            Assert.NotNull(config.CurrentVersion);
            Assert.Equal("4.5.6", config.CurrentVersion);
        }

        [Fact]
        public void Value_from_settings_object_overrides_value_from_environment_variables()
        {
            // ARRANGE
            SetConfigEnvironmentVariable("currentVersion", "1.2.3");

            var settingsObject = new TestSettingsClass()
            {
                CurrentVersion = "4.5.6"
            };

            // ACT
            var config = ChangeLogConfigurationLoader.GetConfiguration(m_ConfigurationFilePath, settingsObject);

            // ASSERT
            Assert.NotNull(config.CurrentVersion);
            Assert.Equal("4.5.6", config.CurrentVersion);
        }

        [Fact]
        public void Value_from_settings_object_overrides_values_from_configuration_file_and_environment_variables()
        {
            // ARRANGE
            SetConfigEnvironmentVariable("currentVersion", "1.2.3");
            PrepareConfiguration("currentVersion", "4.5.6");

            var settingsObject = new TestSettingsClass()
            {
                CurrentVersion = "7.8.9"
            };

            // ACT
            var config = ChangeLogConfigurationLoader.GetConfiguration(m_ConfigurationFilePath, settingsObject);

            // ASSERT
            Assert.NotNull(config.CurrentVersion);
            Assert.Equal("7.8.9", config.CurrentVersion);
        }

        [Flags]
        private enum SettingsTarget
        {
            All = ConfigurationFile | EnvironmentVariables | SettingsObject,
            ConfigurationFile = 0x01,
            EnvironmentVariables = 0x01 << 1,
            SettingsObject = 0x01 << 2
        }

        private static IEnumerable<T> GetEnumValues<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        private static IEnumerable<(object?[] testData, SettingsTarget target)> AllSetValueTestCases()
        {
            static (object?[], SettingsTarget) TestCase(
                string key,
                Expression<Func<ChangeLogConfiguration, object?>> getter,
                object? value,
                SettingsTarget target = SettingsTarget.All,
                Action<ChangeLogConfiguration>? assert = null)
            {
                return (new object?[] { key, getter, value, assert! }, target);
            }

            //
            // Current Version setting
            //
            yield return TestCase("currentVersion", config => config.CurrentVersion, "1.2.3");

            //
            // Version Range setting
            //
            yield return TestCase("versionRange", config => config.VersionRange, "[1.2.3,)");

            //
            // Output Path setting
            //
            yield return TestCase("outputPath", config => config.OutputPath, "outputPath1");

            //
            // Template setting
            //
            foreach (var value in GetEnumValues<ChangeLogConfiguration.TemplateName>())
            {
                yield return TestCase("template:name", config => config.Template.Name, value);
            }

            //
            // Default Template settings
            //
            foreach (var value in GetEnumValues<ChangeLogConfiguration.MarkdownPreset>())
            {
                yield return TestCase("template:default:markdownpreset", config => config.Template.Default.MarkdownPreset, value);
            }

            //
            // Integration provider setting
            //
            foreach (var value in GetEnumValues<ChangeLogConfiguration.IntegrationProvider>())
            {
                yield return TestCase("integrations:provider", config => config.Integrations.Provider, value);
            }

            //
            // GitHub Integration settings
            //
            yield return TestCase("integrations:github:accesstoken", config => config.Integrations.GitHub.AccessToken, "some-value");
            yield return TestCase("integrations:github:remoteName", config => config.Integrations.GitHub.RemoteName, "upstream");
            yield return TestCase("integrations:github:host", config => config.Integrations.GitHub.Host, "example.com");
            yield return TestCase("integrations:github:owner", config => config.Integrations.GitHub.Owner, "some user");
            yield return TestCase("integrations:github:repository", config => config.Integrations.GitHub.Repository, "some-repo");

            //
            // GitLab Integration settings
            //
            yield return TestCase("integrations:gitlab:accesstoken", config => config.Integrations.GitLab.AccessToken, "some-access-token");
            yield return TestCase("integrations:gitlab:remoteName", config => config.Integrations.GitLab.RemoteName, "upstream");
            yield return TestCase("integrations:gitlab:host", config => config.Integrations.GitLab.Host, "example.com");
            yield return TestCase("integrations:gitlab:namespace", config => config.Integrations.GitLab.Namespace, "someuser");
            yield return TestCase("integrations:gitlab:namespace", config => config.Integrations.GitLab.Namespace, "group/subgroup");
            yield return TestCase("integrations:gitlab:project", config => config.Integrations.GitLab.Project, "some-repo");

            //
            // Parser Mode setting
            //
            foreach (var value in GetEnumValues<ChangeLogConfiguration.ParserMode>())
            {
                yield return TestCase("parser:mode", config => config.Parser.Mode, value);
            }

            //
            // Tag Patterns settting
            //
            yield return TestCase("tagpatterns", config => config.TagPatterns, new[] { "pattern1", "pattern2" }, SettingsTarget.ConfigurationFile);

            //
            // Footer settings
            //
            yield return TestCase(
                key: "footers",
                getter: config => config.Footers,
                value: new Dictionary<string, ChangeLogConfiguration.FooterConfiguration>()
                {
                    { "footer1", new ChangeLogConfiguration.FooterConfiguration() { DisplayName = "DisplayName 1" } },
                    { "footer2",  new ChangeLogConfiguration.FooterConfiguration() { DisplayName = "DisplayName 2" } }
                },
                target: SettingsTarget.ConfigurationFile,
                assert: config =>
                {
                    Assert.Contains("footer1", config.Footers.Keys);
                    Assert.Equal("DisplayName 1", config.Footers["footer1"].DisplayName);
                    Assert.Contains("footer2", config.Footers.Keys);
                    Assert.Equal("DisplayName 2", config.Footers["footer2"].DisplayName);
                });

            // overwrite display name for one of the footers the default settings define a display name
            yield return TestCase(
                key: "footers",
                getter: config => config.Footers,
                value: new Dictionary<string, ChangeLogConfiguration.FooterConfiguration>()
                {
                    { "see-also", new ChangeLogConfiguration.FooterConfiguration() { DisplayName = "DisplayName 1" } },
                },
                target: SettingsTarget.ConfigurationFile,
                assert: config =>
                {
                    Assert.Contains("see-also", config.Footers.Keys);
                    Assert.Equal("DisplayName 1", config.Footers["see-also"].DisplayName);
                });

            yield return TestCase(
                key: "footers:footer1:displayName",
                getter: config => config.Footers["footer1"].DisplayName,
                value: "DisplayName 1",
                target: SettingsTarget.All);

            //
            // Scope settings
            //
            yield return TestCase(
                key: "scopes",
                getter: config => config.Scopes,
                value: new Dictionary<string, ChangeLogConfiguration.ScopeConfiguration>()
                {
                    { "scope1", new ChangeLogConfiguration.ScopeConfiguration() { DisplayName = "Display Name 1" } },
                    { "scope2", new ChangeLogConfiguration.ScopeConfiguration() { DisplayName = "Display Name 2" } }
                },
                target: SettingsTarget.ConfigurationFile,
                assert: config =>
                {
                    Assert.Collection(config.Scopes,
                      kvp =>
                      {
                          Assert.Equal("scope1", kvp.Key);
                          Assert.Equal("Display Name 1", kvp.Value.DisplayName);
                      },
                      kvp =>
                      {
                          Assert.Equal("scope2", kvp.Key);
                          Assert.Equal("Display Name 2", kvp.Value.DisplayName);
                      });
                });

            //
            // Entry Types setting
            //
            yield return TestCase(
                key: "entryTypes",
                getter: config => config.EntryTypes,
                value: new Dictionary<string, ChangeLogConfiguration.EntryTypeConfiguration>()
                {
                    { "docs", new ChangeLogConfiguration.EntryTypeConfiguration() { DisplayName = "Documentation Updates", Order = 23 } },
                    { "bugfix", new ChangeLogConfiguration.EntryTypeConfiguration() { DisplayName = "Bug Fixes", Order = 42 } }
                },
                target: SettingsTarget.ConfigurationFile,
                assert: config =>
                {
                    // Values inherited from default config
                    Assert.Contains("feat", config.EntryTypes.Keys);
                    Assert.Contains("fix", config.EntryTypes.Keys);

                    // Values defined in current config
                    Assert.Contains("docs", config.EntryTypes.Keys);
                    Assert.Equal("Documentation Updates", config.EntryTypes["docs"].DisplayName);
                    Assert.Equal(23, config.EntryTypes["docs"].Order);

                    Assert.Contains("bugfix", config.EntryTypes.Keys);
                    Assert.Equal("Bug Fixes", config.EntryTypes["bugfix"].DisplayName);
                    Assert.Equal(42, config.EntryTypes["bugfix"].Order);
                });

            //
            // Filter settings
            //
            yield return TestCase(
                key: "filter:include",
                getter: config => config.Filter.Include,
                value: new[]
                {
                    new ChangeLogConfiguration.FilterExpressionConfiguration() { Type = "docs", Scope = "some-scope"},
                    new ChangeLogConfiguration.FilterExpressionConfiguration() { Type = "ci" },
                },
                target: SettingsTarget.ConfigurationFile,
                assert: config =>
                    Assert.Collection(config.Filter.Include,
                        x => AssertFilterExpression(x, "docs", "some-scope"),
                        x => AssertFilterExpression(x, "ci", "*")
                ));

            yield return TestCase(
                key: "filter:exclude",
                getter: config => config.Filter.Exclude,
                value: new[]
                {
                    new ChangeLogConfiguration.FilterExpressionConfiguration() { Type = "docs", Scope = "some-scope"},
                    new ChangeLogConfiguration.FilterExpressionConfiguration() { Type = "ci" },
                },
                target: SettingsTarget.ConfigurationFile,
                assert: config =>
                    Assert.Collection(config.Filter.Exclude,
                        x => AssertFilterExpression(x, "docs", "some-scope"),
                        x => AssertFilterExpression(x, "ci", "*")
                ));
        }

        public static IEnumerable<object?[]> ConfigurationFileSetValueTestCases()
        {
            foreach (var (testData, target) in AllSetValueTestCases())
            {
                if (target.HasFlag(SettingsTarget.ConfigurationFile))
                {
                    yield return testData;
                }
            }
        }

        public static IEnumerable<object?[]> EnvironmentVariablesSetValueTestCases()
        {
            foreach (var (testData, target) in AllSetValueTestCases())
            {
                if (target.HasFlag(SettingsTarget.EnvironmentVariables))
                {
                    yield return testData;
                }
            }
        }

        public static IEnumerable<object?[]> SettingsObjectSetValueTestCases()
        {
            foreach (var (testData, target) in AllSetValueTestCases())
            {
                if (target.HasFlag(SettingsTarget.SettingsObject))
                {
                    yield return testData;
                }
            }
        }


        [Theory]
        [MemberData(nameof(ConfigurationFileSetValueTestCases))]
        public void Value_can_be_set_in_configuration_file(
            string key,
            Expression<Func<ChangeLogConfiguration, object?>> getter,
            object expectedValue,
            Action<ChangeLogConfiguration>? assert)
        {
            // ARRANGE
            PrepareConfiguration(key, expectedValue);

            // ACT 
            var config = ChangeLogConfigurationLoader.GetConfiguration(m_ConfigurationFilePath);

            // ASSERT
            if (assert is null)
            {
                var actualValue = getter.Compile()(config);
                Assert.Equal(expectedValue, actualValue);
            }
            else
            {
                assert(config);
            }
        }

        [Theory]
        [MemberData(nameof(EnvironmentVariablesSetValueTestCases))]
        public void Value_can_be_set_through_environment_variables(
            string key,
            Expression<Func<ChangeLogConfiguration, object?>> getter,
            object expectedValue,
            Action<ChangeLogConfiguration>? assert)
        {
            // ARRANGE
            SetConfigEnvironmentVariable(key, expectedValue?.ToString());

            // ACT 
            var config = ChangeLogConfigurationLoader.GetConfiguration(m_ConfigurationFilePath);

            // ASSERT
            if (assert is null)
            {
                var actualValue = getter.Compile()(config);
                Assert.Equal(expectedValue, actualValue);
            }
            else
            {
                assert(config);
            }
        }

        [Theory]
        [MemberData(nameof(SettingsObjectSetValueTestCases))]
        public void Value_can_be_set_through_settings_object(
            string key,
            Expression<Func<ChangeLogConfiguration, object?>> getter,
            object expectedValue,
            Action<ChangeLogConfiguration>? assert)
        {
            // ARRANGE
            var cs = $@"
            using Grynwald.Utilities.Configuration;

            public class SettingsObject
            {{
                [ConfigurationValue(""changelog:{key}"")]
                public {expectedValue.GetType().FullName!.Replace("+", ".")} Value {{ get; set; }}

                public SettingsObject({expectedValue.GetType().FullName!.Replace("+", ".")} value)
                {{
                    Value = value;
                }}
            }}
            ";

            var dynamicAssembly = CSharpCompiler.Compile(sourceCode: cs, assemblyName: Guid.NewGuid().ToString());
            var settingsObject = Activator.CreateInstance(dynamicAssembly.GetType("SettingsObject")!, new object[] { expectedValue });

            // ACT 
            var config = ChangeLogConfigurationLoader.GetConfiguration(m_ConfigurationFilePath, settingsObject);

            // ASSERT
            if (assert is null)
            {
                var actualValue = getter.Compile()(config);
                Assert.Equal(expectedValue, actualValue);
            }
            else
            {
                assert(config);
            }
        }

        [Fact]
        public void GetConfiguration_ignores_footer_configuration_if_value_is_array()
        {
            // ARRANGE

            // Before v0.3, the "footers" property was expected to be a array of configuration objects, but
            // was changed to a object (de-serialized into a dictionary).
            // There is no migration for configuration files intended for earlier versions.
            // Configuration values that are not in the expected format must be ignored.

            var json = @"{
                ""changelog"" : {
                    ""footers"" : [
                        { ""name"":  ""some-footer"", ""displayName"":  ""Some Display Name"" }
                    ]
                }
            }";
            File.WriteAllText(m_ConfigurationFilePath, json);

            // ACT 
            var config = ChangeLogConfigurationLoader.GetConfiguration(m_ConfigurationFilePath);

            // ASSERT
            Assert.NotNull(config.Footers);
            Assert.DoesNotContain("some-footer", config.Footers.Keys);
        }

        [Fact]
        public void GetConfiguration_ignores_scope_configuration_if_value_is_array()
        {
            // ARRANGE

            // Before v0.3, the "scopes" property was expected to be a array of configuration objects, but
            // was changed to a object (de-serialized into a dictionary).
            // There is no migration for configuration files intended for earlier versions.
            // Configuration values that are not in the expected format must be ignored.

            var json = @"{
                ""changelog"" : {
                    ""scopes"" : [
                        { ""name"":  ""some-footer"", ""displayName"":  ""Some Display Name"" }
                    ]
                }
            }";
            File.WriteAllText(m_ConfigurationFilePath, json);

            // ACT 
            var config = ChangeLogConfigurationLoader.GetConfiguration(m_ConfigurationFilePath);

            // ASSERT
            Assert.NotNull(config.Scopes);
            Assert.Empty(config.Scopes);
        }

        [Fact]
        public void GetConfiguration_ignores_entry_type_configuration_if_value_is_array()
        {
            // ARRANGE

            // Before v0.3, the "entryTypes" property was expected to be a array of configuration objects, but
            // was changed to a object (de-serialized into a dictionary).
            // There is no migration for configuration files intended for earlier versions.
            // Configuration values that are not in the expected format must be ignored.

            var json = @"{
                ""changelog"" : {
                    ""entryTypes"" : [
                        { ""type"": ""some-type"", ""displayName"": ""Display Name"" }
                    ]
                }
            }";
            File.WriteAllText(m_ConfigurationFilePath, json);

            // ACT 
            var config = ChangeLogConfigurationLoader.GetConfiguration(m_ConfigurationFilePath);

            // ASSERT
            Assert.NotNull(config.EntryTypes);

            // default settings define display names for 2 entry types
            // no other entry types should be present in configuration
            Assert.Equal(2, config.EntryTypes.Count);

            Assert.DoesNotContain("some-type", config.EntryTypes.Keys);
        }
    }
}
