using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ChangeLogCreator.Versions;
using ChangeLogCreator.ConventionalCommits;
using ChangeLogCreator.Git;
using CommandLine;
using Pastel;

namespace ChangeLogCreator
{
    internal class Program
    {
        private class CommandLineParameters
        {
            [Option('r', "repository")]
            public string RepositoryPath { get; set; }

            [Option('o', "outputpath")]
            public string OutputPath { get; set; }
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
            Console.WriteLine($"Loading commits from repository '{parameters.RepositoryPath}'");
            using (var repo = new GitRepository(parameters.RepositoryPath))
            {                
                var latestVersion = new GitTagVersionProvider(repo).AllVersions.OrderByDescending(x => x.Version).First();

                var commits = repo.GetCommits(null, latestVersion.Commit);

                foreach(var commit in commits)
                {
                    Console.Write($"{commit.Id}: ");

                    try
                    {
                        _ = CommitMessageParser.Parse(commit.CommitMessage);
                        Console.WriteLine(" PARSED  ".Pastel("00000").PastelBg("68C355"));
                    }
                    catch (ParserException ex)
                    {
                        Console.WriteLine(" SKIPPED ".Pastel("00000").PastelBg("FF9800") + " " + ex.Message);
                    }
                }
           
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
