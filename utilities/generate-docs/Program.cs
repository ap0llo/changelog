using System;
using System.IO;
using System.Linq;
using System.Reflection;
using CommandLine;

namespace generate_docs
{
    internal class Opts
    {
        [Value(0, Required = true, MetaName = "Root Path")]
        public string RootPath { get; set; } = "";

        [Option("check", Default = false)]
        public bool Check { get; set; } = false;
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


                var output = DocsRenderer.RenderTemplate(inputPath);

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
    }
}
