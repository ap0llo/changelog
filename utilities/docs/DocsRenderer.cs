using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CommandLine;
using Grynwald.ChangeLog;
using Grynwald.ChangeLog.CommandLine;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.Templates;
using Grynwald.ChangeLog.Templates.Default;
using Grynwald.ChangeLog.Templates.GitHubRelease;
using Grynwald.ChangeLog.Templates.GitLabRelease;
using Grynwald.ChangeLog.Templates.Html;
using Grynwald.Utilities.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Scriban;
using Scriban.Runtime;

namespace docs
{
    internal static class DocsRenderer
    {
        /// <summary>
        /// Configuration functions made available to scriban templates
        /// </summary>
        private class ConfigurationFunctions : ScriptObject
        {
            private static readonly JsonSerializerSettings s_JsonSerializerSettings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                ContractResolver = new DefaultContractResolver()
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                }
            };

            public static string? GetScalar(string settingsKey)
            {
                var value = GetConfigurationValue(settingsKey);

                return value switch
                {
                    null => null,
                    string stringValue when String.IsNullOrEmpty(stringValue) => null,
                    string stringValue => stringValue,
                    Enum enumValue => enumValue.ToString(),
                    _ => JsonConvert.SerializeObject(value, s_JsonSerializerSettings)
                };
            }

            public static IEnumerable<object> GetEnumerable(string settingsKey)
            {
                var value = GetConfigurationValue(settingsKey);

                return value switch
                {
                    IEnumerable enumerable => enumerable.Cast<object>(),
                    _ => Enumerable.Empty<object>()
                };
            }

            public static IEnumerable<string> GetAllowedValues(string settingsKey)
            {
                return GetConfigurationValue(settingsKey) switch
                {
                    Enum enumValue => Enum.GetValues(enumValue.GetType()).Cast<object>()?.Select(x => x!.ToString()!)!,
                    _ => Enumerable.Empty<string>()
                };
            }

            public static string GetEnvironmentVariableName(string settingsKey) => settingsKey.Replace(":", "__").ToUpper();

            public static string? GetCommandlineParameter(string settingsKey)
            {
                var property = typeof(CommandLineParameters).GetProperties()
                    .SingleOrDefault(x =>
                        x.GetCustomAttribute<OptionAttribute>() is not null &&
                        x.GetCustomAttribute<ConfigurationValueAttribute>() is ConfigurationValueAttribute configurationAttribute &&
                        configurationAttribute.Key.Equals(settingsKey, StringComparison.OrdinalIgnoreCase)
                    );

                return property?.GetCustomAttribute<OptionAttribute>()?.LongName;
            }


            private static object? GetConfigurationValue(string settingsKey)
            {
                var rootObject = new
                {
                    ChangeLog = ChangeLogConfigurationLoader.GetDefaultConfiguration()
                };

                return GetPropertyValue(rootObject, settingsKey, ':');
            }
        }

        /// <summary>
        /// IEnumerable functions made available to scriban templates
        /// </summary>
        private class EnumerableFunctions : ScriptObject
        {
            public static object? OrderByDescending(IEnumerable toSort, string sortBy)
            {
                return toSort?.Cast<object>()?.OrderByDescending(obj => GetPropertyValue(obj!, sortBy, '.'));
            }
        }

        /// <summary>
        /// Functions that retrieve infomation about changelog templates made available to scriban templates
        /// </summary>
        private class TemplateFunctions : ScriptObject
        {
            public static IEnumerable<string> GetTemplateNames() =>
                Enum.GetValues<ChangeLogConfiguration.TemplateName>().Select(x => x.ToString());

            public static string GetFileTree(string templateName)
            {
                var configuration = ChangeLogConfigurationLoader.GetDefaultConfiguration();

                if (!Enum.TryParse<ChangeLogConfiguration.TemplateName>(templateName, ignoreCase: true, out var parsed))
                    throw new ArgumentException($"Unknown template name '{templateName}'", nameof(templateName));

                ScribanBaseTemplate template = parsed switch
                {
                    ChangeLogConfiguration.TemplateName.Default => new DefaultTemplate(configuration),
                    ChangeLogConfiguration.TemplateName.GitHubRelease => new GitHubReleaseTemplate(configuration),
                    ChangeLogConfiguration.TemplateName.GitLabRelease => new GitLabReleaseTemplate(configuration),
                    ChangeLogConfiguration.TemplateName.Html => new HtmlTemplate(configuration),
                    _ => throw new NotImplementedException()
                };

                return template.FileSystem.ToAsciiTree();
            }
        }

        public static string RenderTemplate(string inputPath)
        {
            if (!IO.HasExtension(inputPath, IO.FileExtensions.Scriban))
                throw new InvalidOperationException($"Expected extension of template file to be '{IO.FileExtensions.Scriban}' but is '{Path.GetExtension(inputPath)}'");

            var input = File.ReadAllText(inputPath);

            var context = new TemplateContext();
            var rootScriptObject = new ScriptObject()
            {
                { "configuration", new ConfigurationFunctions() },
                { "enumerable", new EnumerableFunctions() },
                { "templates", new TemplateFunctions() }
            };

            context.PushGlobal(rootScriptObject);

            var output = new StringBuilder();

            output.AppendLine("<!--");
            output.AppendLine("  <auto-generated>");
            output.AppendLine("    The contents of this file were generated by a tool.");
            output.AppendLine("    Any changes to this file will be overwritten.");
            output.AppendLine($"    To change the content of this file, edit '{Path.GetFileName(inputPath)}'");
            output.AppendLine("  </auto-generated>");
            output.AppendLine("-->");

            var template = Template.Parse(input);
            output.Append(template.Render(context));
            return output.ToString();
        }

        private static object? GetPropertyValue(object configuration, string propertyPath, char separator)
        {
            var propertyNames = propertyPath.Split(separator);

            var currentObject = configuration;
            foreach (var propertyName in propertyNames)
            {
                currentObject = currentObject
                    ?.GetType()
                    ?.GetProperties()
                    ?.SingleOrDefault(p => StringComparer.OrdinalIgnoreCase.Equals(p.Name, propertyName))
                    ?.GetValue(currentObject);
            }

            return currentObject;
        }
    }
}
