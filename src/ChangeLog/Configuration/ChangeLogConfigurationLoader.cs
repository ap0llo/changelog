﻿using System.IO;
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
        public static ChangeLogConfiguration GetConfiguration(string configurationFilePath, object? settingsObject = null)
        {
            using var defaultSettingsStream = GetDefaultSettingsStream();
            using var configurationFileStream = GetFileStreamOrEmpty(configurationFilePath);

            using var preparedStream = PrepareConfigurationFileStream(configurationFileStream);

            return new ConfigurationBuilder()
                .AddJsonStream(defaultSettingsStream)
                // Use AddJsonStream() because AddJsonFile() assumes the file name
                // is relative to the ConfigurationBuilder's base directory and does not seem to properly
                // handle absolute paths
                .AddJsonStream(preparedStream)
                .AddEnvironmentVariables()
                .AddObject(settingsObject)
                .Load();

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

        private static Stream GetFileStreamOrEmpty(string path)
        {
            return File.Exists(path)
                ? File.Open(path, FileMode.Open, FileAccess.Read)
                : (Stream)new MemoryStream(Encoding.ASCII.GetBytes("{ }"));
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
                var footersProperty = changelogSetingsObject?.Property("footers");
                if (footersProperty != null && footersProperty.Value.Type == JTokenType.Array)
                {
                    footersProperty.Remove();
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

#if NETCOREAPP2_1
            // On .NET Core 2.1, a encoding and buffer size MUST be required.
            // UTF-8 without BOM and 1024 are the default values used by StreamWriter
            // (see https://source.dot.net/#System.Private.CoreLib/StreamWriter.cs,74)
            var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
            using (var streamWriter = new StreamWriter(outputStream, encoding, 1024, leaveOpen: true))
#else
            // On .NET Core 3.1 (and presumably later), we can just set encoding and buffer size to null/-1
            // and StreamWriter will use the default values without the need to hard-code them here.
            using (var streamWriter = new StreamWriter(outputStream, null, -1, leaveOpen: true))
#endif
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

