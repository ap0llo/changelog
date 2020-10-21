using System;
using System.IO;
using System.Linq;
using System.Reflection;
using CommandLine;

namespace generate_docs
{
    internal class Program
    {
        private class CommandLineParameterBase
        {
            [Value(0, Required = true, MetaName = "Root Path")]
            public string RootPath { get; set; } = "";

        }

        [Verb("generate")]
        private class GenerateCommandLineParameters : CommandLineParameterBase
        { }

        [Verb("verify")]
        private class VerifyCommandLineParameters : CommandLineParameterBase
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


            var exitCode = parser.ParseArguments<GenerateCommandLineParameters, VerifyCommandLineParameters>(args).MapResult(
                (GenerateCommandLineParameters generateParameters) => GenerateDocs(generateParameters),
                (VerifyCommandLineParameters verifyParameters) => VerifyDocs(verifyParameters),
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

            Console.WriteLine($"Generating documentation from template in '{parameters.RootPath}'");

            var inputFiles = Directory.GetFiles(parameters.RootPath, "*.scriban", SearchOption.AllDirectories);

            foreach (var inputPath in inputFiles)
            {
                Console.WriteLine($"  Processing '{inputPath}'");
                var outputPath = GetTemplateOutputPath(inputPath);

                var output = DocsRenderer.RenderTemplate(inputPath);

                Console.WriteLine($"    Saving output to '{outputPath}'");
                File.WriteAllText(outputPath, output);
            }

            return 0;
        }

        private static int VerifyDocs(VerifyCommandLineParameters parameters)
        {
            if (!ValidateParameters(parameters))
                return 1;

            var success = false;
            // Ensure all scriban templates are rendered
            Console.WriteLine($"Verifying scriban files in '{parameters.RootPath}'");
            var scribanTemplates = Directory.GetFiles(parameters.RootPath, "*.scriban", SearchOption.AllDirectories);
            foreach (var templatePath in scribanTemplates)
            {
                Console.WriteLine($"  Processing '{templatePath}'");
                var outputPath = GetTemplateOutputPath(templatePath);
                if (!File.Exists(outputPath))
                {
                    Console.Error.WriteLine($"    ERR: Template output file does not exists (expected at '{outputPath}')");
                }
            }

            // Validate all markdown file
            Console.WriteLine($"Verifying markdown files in '{parameters.RootPath}'");
            var markdownFiles = Directory.GetFiles(parameters.RootPath, "*.md", SearchOption.AllDirectories);
            foreach (var markdownPath in markdownFiles)
            {
                Console.WriteLine($"  Processing '{markdownPath}'");
                var outputPath = Path.ChangeExtension(markdownPath, "").TrimEnd('.');

                var result = DocsVerifier.VerifyDocument(markdownPath);
                success &= result.Success;

                foreach (var error in result.Errors)
                {
                    Console.Error.WriteLine($"    ERR: {error}");
                }
            }

            return success ? 0 : 1;
        }

        private static bool ValidateParameters(CommandLineParameterBase parameters)
        {
            if (String.IsNullOrWhiteSpace(parameters.RootPath))
            {
                Console.Error.WriteLine("Root Path must not be null or whitespace");
                return false;
            }

            if (!Directory.Exists(parameters.RootPath))
            {
                Console.Error.WriteLine($"Root Path '{parameters.RootPath}' does not exist");
                return false;
            }

            return true;
        }

        private static string GetTemplateOutputPath(string templatePath) => Path.ChangeExtension(templatePath, "").TrimEnd('.');
    }
}
