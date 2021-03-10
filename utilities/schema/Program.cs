using System;
using System.IO;
using System.Linq;
using CommandLine;
using Grynwald.ChangeLog.Configuration;
using Newtonsoft.Json;
using Spectre.Console;

namespace schema
{
    internal class Program
    {
        private class CommandLineParameterBase
        {
            [Value(0, Required = true, MetaName = "Output Path")]
            public string SchemaPath { get; set; } = "";
        }

        [Verb("generate", HelpText = "Generate configuration schema file")]
        private class GenerateCommandLineParameters : CommandLineParameterBase
        { }

        [Verb("validate", HelpText = "Validate configuration schema file is up-to-date")]
        private class ValidateCommandLineParameters : CommandLineParameterBase
        { }


        private static readonly IAnsiConsole StdOut = AnsiConsole.Create(new AnsiConsoleSettings()
        {
            Ansi = AnsiSupport.Detect,
            ColorSystem = ColorSystemSupport.Detect,
            Out = Console.Out
        });
        private static readonly IAnsiConsole StdErr = AnsiConsole.Create(new AnsiConsoleSettings()
        {
            Ansi = AnsiSupport.Detect,
            ColorSystem = ColorSystemSupport.Detect,
            Out = Console.Error
        });
        private static readonly Style s_MessageStyle = new Style(foreground: Color.Green);
        private static readonly Style s_ErrorStyle = new Style(foreground: Color.Red);


        private static int Main(string[] args)
        {
            var parser = new Parser(config =>
            {
                config.CaseSensitive = false;
                config.CaseInsensitiveEnumValues = true;
                config.HelpWriter = Console.Out;
            });

            var exitCode = parser.ParseArguments<GenerateCommandLineParameters, ValidateCommandLineParameters>(args).MapResult(
                (GenerateCommandLineParameters generateParameters) => GenerateSchema(generateParameters),
                (ValidateCommandLineParameters validateParameters) => ValidateSchema(validateParameters),
                errors =>
                {
                    if (errors.All(e => e is HelpRequestedError || e is VersionRequestedError))
                    {
                        return 0;
                    }

                    WriteError("ERROR: Invalid arguments");
                    return 1;
                });


            if (exitCode == 0)
            {
                WriteMessage("Execution completed successfully.");
            }
            else
            {
                WriteError("Execution completed with errors.");
            }

            return exitCode;
        }


        private static int GenerateSchema(GenerateCommandLineParameters parameters)
        {
            if (!ValidateParameters(parameters))
                return 1;

            WriteMessage("Generating configuration file JSON schema");
            var schema = GetSchema();

            var schemaPath = Path.GetFullPath(parameters.SchemaPath);

            WriteMessage($"Saving schema to '{schemaPath}'");

            Directory.CreateDirectory(Path.GetDirectoryName(schemaPath)!);
            File.WriteAllText(schemaPath, schema);
            return 0;
        }

        private static int ValidateSchema(ValidateCommandLineParameters parameters)
        {
            if (!ValidateParameters(parameters))
                return 1;

            var schemaPath = Path.GetFullPath(parameters.SchemaPath);
            WriteMessage($"Validating configuration file JSON schema at '{schemaPath}'");

            if (!File.Exists(schemaPath))
            {
                WriteError($"ERROR: Schema file at '{schemaPath}' does not exist");
                return 1;
            }

            var expectedSchema = GetSchema();
            var actualSchema = File.ReadAllText(schemaPath);

            if (!StringComparer.Ordinal.Equals(expectedSchema, actualSchema))
            {
                WriteError("ERROR: Schema file differs from expected schema");
                return 1;
            }

            return 0;
        }

        private static bool ValidateParameters(CommandLineParameterBase parameters)
        {
            if (String.IsNullOrWhiteSpace(parameters.SchemaPath))
            {
                WriteError("ERROR: No output path specified");
                return false;
            }

            return true;
        }

        private static string GetSchema()
        {
            var rootObject = new
            {
                changelog = ChangeLogConfigurationLoader.GetDefaultConfiguration()
            };
            return JsonSchemaGenerator.GetSchema(rootObject).ToString(Formatting.Indented);
        }

        private static void WriteMessage(string message) => StdOut.WriteLine(message, s_MessageStyle);

        private static void WriteError(string message) => StdErr.WriteLine(message, s_ErrorStyle);
    }
}
