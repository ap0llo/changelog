using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ChangeLogCreator.Configuration;
using ChangeLogCreator.Git;
using ChangeLogCreator.Tasks;
using CommandLine;

namespace ChangeLogCreator
{
    internal class Program
    {

        private static int Main(string[] args)
        {
            using var commandlineParser = new Parser(settings =>
            {
                settings.CaseInsensitiveEnumValues = true;
                settings.CaseSensitive = false;
                settings.HelpWriter = Console.Out;
            });

            return commandlineParser.ParseArguments<CommandLineParameters>(args)
                .MapResult(
                    (CommandLineParameters parsed) => Run(parsed),
                    (IEnumerable<Error> errors) =>
                    {
                        if (errors.All(e => e is HelpRequestedError || e is VersionRequestedError))
                            return 0;

                        Console.Error.WriteLine("Invalid arguments");
                        return 1;
                    }
                );
        }


        private static int Run(CommandLineParameters commandlineParameters)
        {
            if (!ValidateCommandlineParameters(commandlineParameters))
                return 1;

            var configuration = ChangeLogConfigurationLoader.GetConfiguation(commandlineParameters.RepositoryPath, commandlineParameters);

            using (var repo = new GitRepository(commandlineParameters.RepositoryPath))
            {
                var pipeline = new ChangeLogPipeline()
                    .AddTask(new LoadVersionsTask(configuration, repo))
                    .AddTask(new ParseCommitsTask(repo))
                    .AddTask(new RenderMarkdownTask(configuration));

                pipeline.Run();
            }

            return 0;
        }


        private static bool ValidateCommandlineParameters(CommandLineParameters parameters)
        {
            if (String.IsNullOrEmpty(parameters.RepositoryPath))
                return false;
            else
                return Directory.Exists(parameters.RepositoryPath);
        }
    }
}
