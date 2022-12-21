using System;
using System.IO;
using System.Reflection;
using System.Text;
using Grynwald.Utilities.Configuration;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Grynwald.ChangeLog.Configuration
{
    internal static class ChangeLogConfigurationLoader
    {
        public static ChangeLogConfiguration GetConfiguration(string? configurationFilePath, params object[] settingsObjects)
        {
            using var defaultSettingsStream = GetDefaultSettingsStream();
            using var configurationFileStream = GetFileStreamOrEmpty(configurationFilePath);

            using var preparedStream = PrepareConfigurationFileStream(configurationFileStream);


            var configurationBuilder = new ConfigurationBuilder()
                .AddJsonStream(defaultSettingsStream)
                // Use AddJsonStream() because AddJsonFile() assumes the file name
                // is relative to the ConfigurationBuilder's base directory and does not seem to properly
                // handle absolute paths
                .AddJsonStream(preparedStream)
                .AddEnvironmentVariables();

            foreach (var settingsObject in settingsObjects)
            {
                configurationBuilder = configurationBuilder.AddObject(settingsObject);
            }

            return configurationBuilder.Load();
        }

        internal static ChangeLogConfiguration GetDefaultConfiguration()
        {
            using var defaultSettingsStream = GetDefaultSettingsStream();

            return new ConfigurationBuilder()
                .AddJsonStream(defaultSettingsStream)
                .Load();
        }


        private static Stream? GetDefaultSettingsStream() =>
            Assembly.GetExecutingAssembly().GetManifestResourceStream("Grynwald.ChangeLog.Configuration.defaultSettings.json");

        private static Stream GetFileStreamOrEmpty(string? path)
        {
            return !String.IsNullOrEmpty(path) && File.Exists(path)
                ? File.Open(path, FileMode.Open, FileAccess.Read)
                : new MemoryStream(Encoding.ASCII.GetBytes("{ }"));
        }

        private static ChangeLogConfiguration Load(this IConfigurationBuilder builder)
        {
            var configuration = new ChangeLogConfiguration();
            builder
                .Build()
                .GetSection("changelog")
                .Bind(configuration);

            return configuration;
        }

        /// <summary>
        /// Prepares the configuration file stream for processing by the configuration binder.
        /// </summary>
        /// <remarks>
        /// Performs required adjustments to the JSON file in case the file does not follow the currently expected file structure
        /// but is a configuration file intended for earlier versions of changelog.
        /// </remarks>
        private static Stream PrepareConfigurationFileStream(Stream configFileStream)
        {
            var json = ReadJsonFromStream(configFileStream);

            if (json is JObject jobject && jobject.Property("changelog")?.Value is JObject changelogSetingsObject)
            {
                // Before version 0.3, the "footers" property was a JSON array.
                // In version 0.3 the array was replaced by a JSON object in the configuration file
                // To avoid unexpected behavior when loading a "old" configuration file
                // (Microsoft.Extensions.Configuration will try to bind the array to the dictionary v0.3+ uses)
                // remove the array from the JSON if it is present.
                {
                    var footersProperty = changelogSetingsObject?.Property("footers");
                    if (footersProperty is not null && footersProperty.Value.Type == JTokenType.Array)
                    {
                        footersProperty.Remove();
                    }
                }

                // Before version 0.3, the "scopes" property was a JSON array.
                // In version 0.3 the array was replaced by a JSON object in the configuration file
                // To avoid unexpected behavior when loading a "old" configuration file
                // (Microsoft.Extensions.Configuration will try to bind the array to the dictionary v0.3+ uses)
                // remove the array from the JSON if it is present.
                {
                    var scopesProperty = changelogSetingsObject?.Property("scopes");
                    if (scopesProperty is not null && scopesProperty.Value.Type == JTokenType.Array)
                    {
                        scopesProperty.Remove();
                    }
                }

                // Before version 0.3, the "entryTypes" property was a JSON array.
                // In version 0.3 the array was replaced by a JSON object in the configuration file
                // To avoid unexpected behavior when loading a "old" configuration file
                // (Microsoft.Extensions.Configuration will try to bind the array to the dictionary v0.3+ uses)
                // remove the array from the JSON if it is present.
                {
                    var entryTypesProperty = changelogSetingsObject?.Property("entryTypes");
                    if (entryTypesProperty is not null && entryTypesProperty.Value.Type == JTokenType.Array)
                    {
                        entryTypesProperty.Remove();
                    }
                }
            }

            return SaveJsonToStream(json);
        }

        /// <summary>
        /// Reads the specified Stream as JSON
        /// </summary>
        private static JToken ReadJsonFromStream(Stream stream)
        {
            using var streamReader = new StreamReader(stream);
            using var reader = new JsonTextReader(streamReader);

            var json = JToken.ReadFrom(reader);

            return json;
        }

        /// <summary>
        /// Writes the specified JSON to a MemoryStream.
        /// </summary>
        private static Stream SaveJsonToStream(JToken json)
        {
            var outputStream = new MemoryStream();

            using (var streamWriter = new StreamWriter(outputStream, null, -1, leaveOpen: true))
            using (var writer = new JsonTextWriter(streamWriter))
            {
                json.WriteTo(writer);
            }

            // Reset Memory-Stream to make it readable
            outputStream.Seek(0, SeekOrigin.Begin);
            return outputStream;
        }
    }
}

