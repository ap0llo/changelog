using CommandLine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Scriban;
using Scriban.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

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




        public static string? Get(string key)
        {
            JObject? defaultSettings = LoadDefaultSettings();
            JToken? defaultValue = GetPropertyValue(defaultSettings, key);

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
            JObject? defaultSettings = LoadDefaultSettings();
            JObject? entryTypes = GetPropertyValue(defaultSettings, "changelog:entryTypes") as JObject;

            foreach(var property in entryTypes?.Properties() ?? Enumerable.Empty<JProperty>())
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


        private static JObject LoadDefaultSettings()
        {
            using Stream? resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(s_ResourceName);
            if (resourceStream is null)
            {
                throw new InvalidOperationException($"Failed to open stream for resource '{s_ResourceName}'");
            }

            using StreamReader? streamReader = new StreamReader(resourceStream);
            using JsonTextReader? jsonReader = new JsonTextReader(streamReader);

            return (JObject)JToken.ReadFrom(jsonReader);
        }

        private static JToken? GetPropertyValue(JObject jsonObject, string propertyPath)
        {
            string[]? propertyNames = propertyPath.Split(':');
            JToken? currentObject = jsonObject;
            foreach (string? propertyName in propertyNames)
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
            string[]? resources = Assembly.GetExecutingAssembly().GetManifestResourceNames();

            Parser? parser = new Parser(config =>
            {
                config.CaseSensitive = false;
                config.CaseInsensitiveEnumValues = true;
                config.HelpWriter = Console.Out;
            });


            int exitCode = parser.ParseArguments<Opts>(args).MapResult(
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
            if (string.IsNullOrWhiteSpace(opts.RootPath))
            {
                Console.Error.WriteLine("Root Path must not be null or whitespace");
                return 1;
            }

            if (!Directory.Exists(opts.RootPath))
            {
                Console.Error.WriteLine($"Root Path '{opts.RootPath}' does not exist");
                return 1;
            }

            string[]? inputFiles = Directory.GetFiles(opts.RootPath, "*.scriban", SearchOption.AllDirectories);

            bool success = true;

            foreach (string? inputPath in inputFiles)
            {
                Console.WriteLine($"Processing '{inputPath}'");
                string? outputPath = Path.ChangeExtension(inputPath, "").TrimEnd('.');

                if (opts.Check && !File.Exists(outputPath))
                {
                    Console.Error.WriteLine($"Expected output file '{outputPath}' does not exist");
                    success = false;
                    continue;
                }

                string? input = File.ReadAllText(inputPath);
                string? output = RenderTemplate(input);

                if (opts.Check)
                {
                    string? currentOutput = File.ReadAllText(outputPath);
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


        private static string RenderTemplate(string input)
        {
            TemplateContext? context = new TemplateContext();
            DefaultSettingsFunctions? rootScriptObject = new DefaultSettingsFunctions()
            {
                { "defaultSettings", new DefaultSettingsFunctions() }
            };

            context.PushGlobal(rootScriptObject);

            Template? template = Template.Parse(input);
            string? result = template.Render(context);
            return result;
        }
    }
}
