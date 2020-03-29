using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Grynwald.ChangeLog.Configuration
{
    internal static class ChangeLogConfigurationLoader
    {
        public static ChangeLogConfiguration GetConfiguation(string configurationFilePath, object? settingsObject = null)
        {
            using var defaultSettingsStream = GetDefaultSettingsStream();
            using var configurationFileStream = GetFileStreamOrEmpty(configurationFilePath);

            return new ConfigurationBuilder()
                .AddJsonStream(defaultSettingsStream)
                // Use AddJsonStream() because AddJsonFile() assumes the file name
                // is relative to the ConfigurationBuilder's base directory and does not seem to properly
                // handle absolute paths
                .AddJsonStream(configurationFileStream)
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
    }
}

