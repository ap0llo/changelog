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

            var inputFiles = Directory.GetFiles(parameters.RootPath, "*.*", SearchOption.AllDirectories).Where(x => IO.HasExtension(x, IO.FileExtensions.Scriban));

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

        private static int VerifyDocs(VerifyCommandLineParameters parameters)
        {
            if (!ValidateParameters(parameters))
                return 1;

            var success = false;

            Console.WriteLine($"Verifying files in '{parameters.RootPath}'");

            foreach (var path in Directory.GetFiles(parameters.RootPath, "*.*", SearchOption.AllDirectories))
            {
                Console.WriteLine($"  Processing '{path}'");
                var outputPath = Path.ChangeExtension(path, "").TrimEnd('.');

                var result = DocsVerifier.VerifyDocument(path);
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
    }
}
