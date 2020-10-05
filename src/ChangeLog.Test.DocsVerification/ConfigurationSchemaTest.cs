using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Sdk;

namespace Grynwald.ChangeLog.Test.DocsVerification
{
    /// <summary>
    /// Tests verifying the configuration file JSON schema.
    /// </summary>
    /// <remarks>
    /// For convenience, the default values of some settings are also included as default value in the settings schema
    /// so editors with auto completion can show the default value when editing a configuration file.
    /// To avoid mismatches between the default settings and the default values from the schema, the test in this file make sure
    /// both files use the same default value.
    /// </remarks>
    public class ConfigurationSchemaTest : TestBase
    {
        private const string s_SchemaFilePath = "schemas/configuration/schema.json";
        private const string s_DefaultSettingsFilePath = "src/ChangeLog/Configuration/defaultSettings.json";

        public static IEnumerable<object[]> PropertiesToCompare()
        {
            object[] TestCase(string setttingsKey, string schemaKey)
            {
                return new object[] { setttingsKey, schemaKey };
            }

            IEnumerable<object[]> ChildProperyTestCases(string settingsKey, string schemaKey)
            {
                var allProperties = Enumerable.Union(
                    GetSettingsProperties(settingsKey),
                    GetSchemaProperties(schemaKey)
                );
                foreach (var propertyName in allProperties)
                {
                    yield return TestCase($"{settingsKey}:{propertyName}", $"{schemaKey}:{propertyName}:default");
                }
            }


            // for the "entryTypes" setting, do not validate the whole object but the defaults of the individual properties
            foreach (var testCase in ChildProperyTestCases("changelog:entryTypes", "definitions:changelogConfiguration:properties:entryTypes:properties"))
            {
                yield return testCase;
            }

            // for the "footers" setting, do not validate the whole object but the defaults of the individual properties
            foreach (var testCase in ChildProperyTestCases("changelog:footers", "definitions:changelogConfiguration:properties:footers:properties"))
            {
                yield return testCase;
            }

            yield return TestCase($"changelog:tagPatterns", $"definitions:changelogConfiguration:properties:tagPatterns:default");
            yield return TestCase($"changelog:scopes", $"definitions:changelogConfiguration:properties:scopes:default");
            yield return TestCase($"changelog:outputPath", $"definitions:changelogConfiguration:properties:outputPath:default");

            // for the "filter" setting, do not validate the whole object but the defaults of the individual properties
            foreach (var testCase in ChildProperyTestCases("changelog:filter", "definitions:filterConfiguration:properties"))
            {
                yield return testCase;
            }
        }


        [Theory]
        [MemberData(nameof(PropertiesToCompare))]
        public void Schema_default_values_and_default_settings_match(string settingsPropertyKey, string schemaPropertyKey)
        {
            var schemaValue = GetSchemaValue(schemaPropertyKey);
            var settingsValue = GetDefaultSetttingsValue(settingsPropertyKey);

            Assert.False(schemaValue is null, $"Default value of '{schemaPropertyKey}' is missing from schema");
            Assert.False(settingsValue is null, $"Default value of '{settingsPropertyKey}' is missing from default settings");

            if (!JToken.DeepEquals(schemaValue, settingsValue))
            {
                throw new XunitException(
                    $"Default values in schema and default settings differ:{Environment.NewLine}" +
                    Environment.NewLine +
                    $"Schema:{Environment.NewLine}" +
                    schemaValue!.ToString() +
                    Environment.NewLine +
                    Environment.NewLine +
                    $"Default Settings:{Environment.NewLine}" +
                    settingsValue!.ToString());
            }
        }


        private static JToken? GetPropertyValue(JObject jsonObject, string propertyPath)
        {
            var propertyNames = propertyPath.Split(':');
            JToken? currentObject = jsonObject;
            foreach (var propertyName in propertyNames)
            {
                currentObject = currentObject?[propertyName];
            }

            return currentObject;
        }

        private static JToken? GetSchemaValue(string propertyPath)
        {
            var schema = JObject.Parse(File.ReadAllText(Path.Combine(RootPath, s_SchemaFilePath)));
            return GetPropertyValue(schema, propertyPath);
        }

        private static JToken? GetDefaultSetttingsValue(string propertyPath)
        {
            var defaultSettings = JObject.Parse(File.ReadAllText(Path.Combine(RootPath, s_DefaultSettingsFilePath)));
            return GetPropertyValue(defaultSettings, propertyPath);
        }

        private static IEnumerable<string> GetSchemaProperties(string parentObjectPath)
        {
            var schema = JObject.Parse(File.ReadAllText(Path.Combine(RootPath, s_SchemaFilePath)));
            var value = GetPropertyValue(schema, parentObjectPath);

            if (value is JObject objectValue)
            {
                return objectValue.Properties().Select(x => x.Name);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private static IEnumerable<string> GetSettingsProperties(string parentObjectPath)
        {
            var defaultSettings = JObject.Parse(File.ReadAllText(Path.Combine(RootPath, s_DefaultSettingsFilePath)));
            var value = GetPropertyValue(defaultSettings, parentObjectPath);

            if (value is JObject objectValue)
            {
                return objectValue.Properties().Select(x => x.Name);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

    }
}
