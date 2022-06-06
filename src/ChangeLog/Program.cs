using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using Grynwald.ChangeLog.CommandLine;
using Grynwald.ChangeLog.Commands;

namespace Grynwald.ChangeLog
{
    internal static class Program
    {
        private static async Task<int> Main(string[] args)
        {
            return await CommandLineParser.Parse(args)
                .MapResult(
                    (GenerateCommandLineParameters parsed) =>
                    {
                        var command = new GenerateCommand(parsed);
                        return command.RunAsync();
                    },
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
