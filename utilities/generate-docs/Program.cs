using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CommandLine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Scriban;
using Scriban.Runtime;

namespace generate_docs
{
    internal class Opts
    {
        [Value(0, Required = true, MetaName = "Root Path")]
        public string RootPath { get; set; } = "";

        [Option("check", Default = false)]
        public bool Check { get; set; } = false;
    }


    internal class DefaultSettingsFunctions : ScriptObject
    {
        private const string s_ResourceName = "generate_docs.defaultSettings.json";

        public class EntryType
        {
            public string Type { get; }

            public int? Priority { get; set; }

            public string? DisplayName { get; set; }

            public EntryType(string type)
            {
                Type = type;
            }
        }

        public class Footer
        {
            public string Name { get; }


            public string? DisplayName { get; set; }

            public Footer(string name)
            {
                Name = name;
            }
        }





        public static string? Get(string key)
        {
            var defaultSettings = LoadDefaultSettings();
            var defaultValue = GetPropertyValue(defaultSettings, key);

            if (defaultValue is null)
            {
                return null;
            }

            if (defaultValue.Type == JTokenType.Null)
            {
                return null;
            }

            if (defaultValue is JValue jvalue)
            {
                return jvalue.Value?.ToString();
            }

            return defaultValue.ToString(Formatting.Indented);
        }

        public static IEnumerable<EntryType> GetEntryTypes()
        {
            var defaultSettings = LoadDefaultSettings();
            var entryTypes = GetPropertyValue(defaultSettings, "changelog:entryTypes") as JObject;

            foreach (var property in entryTypes?.Properties() ?? Enumerable.Empty<JProperty>())
            {
                var priority = (property.Value as JObject)?["priority"]?.Value<int>();
                var displayName = (property.Value as JObject)?["displayName"]?.Value<string>();

                yield return new EntryType(property.Name)
                {
                    Priority = priority,
                    DisplayName = displayName
                };
            }
        }

        public static IEnumerable<Footer> GetFooters()
        {
            var defaultSettings = LoadDefaultSettings();
            var entryTypes = GetPropertyValue(defaultSettings, "changelog:footers") as JObject;

            foreach (var property in entryTypes?.Properties() ?? Enumerable.Empty<JProperty>())
            {
                var displayName = (property.Value as JObject)?["displayName"]?.Value<string>();

                yield return new Footer(property.Name)
                {
                    DisplayName = displayName
                };
            }
        }


        private static JObject LoadDefaultSettings()
        {
            using var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(s_ResourceName);
            if (resourceStream is null)
            {
                throw new InvalidOperationException($"Failed to open stream for resource '{s_ResourceName}'");
            }

            using var streamReader = new StreamReader(resourceStream);
            using var jsonReader = new JsonTextReader(streamReader);

            return (JObject)JToken.ReadFrom(jsonReader);
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

    }


    internal class Program
    {
        private static int Main(string[] args)
        {
            var resources = Assembly.GetExecutingAssembly().GetManifestResourceNames();

            var parser = new Parser(config =>
            {
                config.CaseSensitive = false;
                config.CaseInsensitiveEnumValues = true;
                config.HelpWriter = Console.Out;
            });


            var exitCode = parser.ParseArguments<Opts>(args).MapResult(
                opts => Run(opts),
                errors =>
                {
                    if (errors.All(e => e is HelpRequestedError || e is VersionRequestedError))
                    {
                        return 0;
                    }

                    Console.Error.WriteLine("Invalid arguments");
                    return 1;
                });

            Console.WriteLine("Execution completed.");
            return exitCode;
        }

        private static int Run(Opts opts)
        {
            if (String.IsNullOrWhiteSpace(opts.RootPath))
            {
                Console.Error.WriteLine("Root Path must not be null or whitespace");
                return 1;
            }

            if (!Directory.Exists(opts.RootPath))
            {
                Console.Error.WriteLine($"Root Path '{opts.RootPath}' does not exist");
                return 1;
            }

            var inputFiles = Directory.GetFiles(opts.RootPath, "*.scriban", SearchOption.AllDirectories);

            var success = true;

            foreach (var inputPath in inputFiles)
            {
                Console.WriteLine($"Processing '{inputPath}'");
                var outputPath = Path.ChangeExtension(inputPath, "").TrimEnd('.');

                if (opts.Check && !File.Exists(outputPath))
                {
                    Console.Error.WriteLine($"Expected output file '{outputPath}' does not exist");
                    success = false;
                    continue;
                }

                var input = File.ReadAllText(inputPath);
                var output = RenderTemplate(input, Path.GetFileName(inputPath));

                if (opts.Check)
                {
                    var currentOutput = File.ReadAllText(outputPath);
                    if (!StringComparer.Ordinal.Equals(currentOutput, output))
                    {
                        Console.Error.WriteLine($"Contents of '{outputPath}' are not up-to-date");
                        success = false;
                        continue;
                    }

                }
                else
                {
                    Console.WriteLine($"Saving output to '{outputPath}'");
                    File.WriteAllText(outputPath, output);
                }

            }

            return success ? 0 : 1;
        }


        private static string RenderTemplate(string input, string fileName)
        {
            var context = new TemplateContext();
            var rootScriptObject = new DefaultSettingsFunctions()
            {
                { "defaultSettings", new DefaultSettingsFunctions() }
            };

            context.PushGlobal(rootScriptObject);

            var output = new StringBuilder();

            output.AppendLine("<!--");
            output.AppendLine("  <auto-generated>");
            output.AppendLine("    The contents of this file were generated by a tool.");
            output.AppendLine("    Any changes to this file will be overwritten.");
            output.AppendLine($"    To change the content of this file, edit '{fileName}'");
            output.AppendLine("  </auto-generated>");
            output.AppendLine("-->");

            var template = Template.Parse(input);
            output.Append(template.Render(context));
            return output.ToString();
        }
    }
}
