﻿using System;
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
        private class CommandLineParameters
        {
            [Option('r', "repository", Required = true)]
            public string RepositoryPath { get; set; } = "";

            [Option('o', "outputpath", Required = true)]
            public string OutputPath { get; set; } = "";
        }

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


        private static int Run(CommandLineParameters parameters)
        {
            if (!ValidateCommandlineParameters(parameters))
                return 1;

            var configuration = ChangeLogConfigurationLoader.GetConfiguation(parameters.RepositoryPath);

            using (var repo = new GitRepository(parameters.RepositoryPath))
            {
                var pipeline = new ChangeLogPipeline()
                    .AddTask(new LoadVersionsTask(configuration, repo))
                    .AddTask(new ParseCommitsTask(repo))
                    .AddTask(new RenderMarkdownTask(parameters.OutputPath, configuration));

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
