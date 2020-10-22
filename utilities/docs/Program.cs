using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CommandLine;
using Spectre.Console;

namespace docs
{
    internal static class Program
    {
        private class CommandLineParameterBase
        {
            [Value(0, Required = true, MetaName = "Root Path")]
            public IEnumerable<string> InputPaths { get; set; } = Enumerable.Empty<string>();

        }

        [Verb("generate", HelpText = "Update all generated documentation files")]
        private class GenerateCommandLineParameters : CommandLineParameterBase
        { }

        [Verb("validate", HelpText = "Validate all documentation files")]
        private class ValidateCommandLineParameters : CommandLineParameterBase
        { }


        private static int Main(string[] args)
        {
            var resources = Assembly.GetExecutingAssembly().GetManifestResourceNames();

            var parser = new Parser(config =>
            {
                config.CaseSensitive = false;
                config.CaseInsensitiveEnumValues = true;
                config.HelpWriter = Console.Out;
            });


            var exitCode = parser.ParseArguments<GenerateCommandLineParameters, ValidateCommandLineParameters>(args).MapResult(
                (GenerateCommandLineParameters generateParameters) => GenerateDocs(generateParameters),
                (ValidateCommandLineParameters validateParameters) => ValidateDocs(validateParameters),
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

        private static int GenerateDocs(GenerateCommandLineParameters parameters)
        {
            if (!ValidateParameters(parameters))
                return 1;

            Console.WriteLine($"Generating documentation from templates");

            var inputFiles = GetAllInputFiles(parameters).Where(x => IO.HasExtension(x, IO.FileExtensions.Scriban));

            foreach (var inputPath in inputFiles)
            {
                Console.WriteLine($"  Processing '{inputPath}'");
                var outputPath = IO.GetTemplateOutputPath(inputPath);

                var output = DocsRenderer.RenderTemplate(inputPath);

                Console.WriteLine($"    Saving output to '{outputPath}'");
                File.WriteAllText(outputPath, output);
            }

            return 0;
        }

        private static int ValidateDocs(ValidateCommandLineParameters parameters)
        {
            if (!ValidateParameters(parameters))
                return 1;

            var success = true;

            var resultsTable = new Table()
                .SetBorder(TableBorder.Square)
                .SetBorderColor(Color.White)
                .AddColumn(new TableColumn("[u]File[/]").LeftAligned())
                .AddColumn(new TableColumn("[u]Result[/]").LeftAligned())
                .AddColumn(new TableColumn("[u]LineNumber[/]").LeftAligned())
                .AddColumn(new TableColumn("[u]RuleId[/]").LeftAligned())
                .AddColumn(new TableColumn("[u]Message[/]").LeftAligned());


            foreach (var path in GetAllInputFiles(parameters))
            {
                var relativePath = path;

                var outputPath = Path.ChangeExtension(path, "").TrimEnd('.');

                var result = DocsValidator.ValidateDocument(path);
                success &= result.Success;


                if (result.Success)
                {
                    resultsTable.AddRow(
                        $"[green]{relativePath}[/]",
                        "[green]Success[/]"
                    );
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        resultsTable.AddRow(
                            $"[red]{relativePath}[/]",
                            "[red]Error[/]",
                            $"{(error.LineNumber > 0 ? error.LineNumber.ToString() : "")}",
                            $"{error.RuleId}",
                            $"{error.Message}"
                        );
                    }
                }
            }

            AnsiConsole.Render(resultsTable);
            return success ? 0 : 1;
        }

        private static bool ValidateParameters(CommandLineParameterBase parameters)
        {
            if (!parameters.InputPaths.Any())
            {
                Console.Error.WriteLine("No input paths specified");
                return false;
            }

            var success = true;
            foreach (var inputPath in parameters.InputPaths)
            {
                if (String.IsNullOrWhiteSpace(inputPath))
                {
                    Console.Error.WriteLine("Input path must not be null or whitespace");
                    success = false;
                }
                else if (!Directory.Exists(inputPath) && !File.Exists(inputPath))
                {
                    Console.Error.WriteLine($"Input path '{inputPath}' does not exist");
                    success = false;
                }
            }

            return success;
        }

        private static IEnumerable<string> GetAllInputFiles(CommandLineParameterBase parameters)
        {
            foreach (var inputPath in parameters.InputPaths)
            {
                if (Directory.Exists(inputPath))
                {
                    foreach (var path in Directory.GetFiles(inputPath, "*.*", SearchOption.AllDirectories))
                    {
                        yield return path;
                    }
                }
                else if (File.Exists(inputPath))
                {
                    yield return inputPath;
                }
                else
                {
                    throw new FileNotFoundException($"Input path '{inputPath}' does not exist");
                }
            }
        }
    }
}
