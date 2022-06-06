using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using Grynwald.ChangeLog.CommandLine;

namespace Grynwald.ChangeLog
{
    internal static class Program
    {
        private static async Task<int> Main(string[] args)
        {
            using var compositionRoot = new CompositionRoot();

            return await CommandLineParser.Parse(args)
                .MapResult(
                    async (GenerateCommandLineParameters generateParameters) => await compositionRoot.CreateGenerateCommand(generateParameters).RunAsync(),
                    (DummyCommandLineParameters dummy) =>
                    {
                        Console.Error.WriteLine("Invalid arguments");
                        return Task.FromResult(1);
                    },
                    (IEnumerable<Error> errors) =>
                    {
                        if (errors.All(e => e is HelpRequestedError || e is HelpVerbRequestedError || e is VersionRequestedError))
                            return Task.FromResult(0);

                        Console.Error.WriteLine("Invalid arguments");
                        return Task.FromResult(1);
                    }
                );
        }
    }
}
