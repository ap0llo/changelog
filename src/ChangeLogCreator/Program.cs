using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ChangeLogCreator.Configuration;
using ChangeLogCreator.Git;
using ChangeLogCreator.Integrations.GitHub;
using ChangeLogCreator.Tasks;
using CommandLine;
using Octokit;

namespace ChangeLogCreator
{
    internal static class Program
    {
        private static async Task<int> Main(string[] args)
        {
            using var commandlineParser = new Parser(settings =>
            {
                settings.CaseInsensitiveEnumValues = true;
                settings.CaseSensitive = false;
                settings.HelpWriter = Console.Out;
            });

            return await commandlineParser.ParseArguments<CommandLineParameters>(args)
                .MapResult(
                    (CommandLineParameters parsed) => RunAsync(parsed),
                    (IEnumerable<Error> errors) =>
                    {
                        if (errors.All(e => e is HelpRequestedError || e is VersionRequestedError))
                            return Task.FromResult(0);

                        Console.Error.WriteLine("Invalid arguments");
                        return Task.FromResult(1);
                    }
                );
        }


        private static async Task<int> RunAsync(CommandLineParameters commandlineParameters)
        {
            if (!ValidateCommandlineParameters(commandlineParameters))
                return 1;

            //TODO: Use a DI container

            var configuration = ChangeLogConfigurationLoader.GetConfiguation(commandlineParameters.RepositoryPath, commandlineParameters);


            using (var gitRepository = new GitRepository(commandlineParameters.RepositoryPath))
            {
                var pipeline = new ChangeLogPipeline()
                    .AddTask(new LoadVersionsTask(configuration, gitRepository))
                    .AddTask(new ParseCommitsTask(gitRepository))
                    .AddIntegrationTasks(configuration, gitRepository)
                    .AddTask(new RenderMarkdownTask(configuration));

                await pipeline.RunAsync();
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

        private static ChangeLogPipeline AddIntegrationTasks(this ChangeLogPipeline pipeline, ChangeLogConfiguration configuration, IGitRepository gitRepository)
        {
            if (configuration.Integrations.Provider == ChangeLogConfiguration.IntegrationProvider.GitHub)
            {
                var githubClient = new GitHubClient(new ProductHeaderValue("changelog-creator")); //TODO: Use a less-generic name.

                if (!String.IsNullOrEmpty(configuration.Integrations.GitHub.AccessToken))
                {
                    githubClient.Credentials = new Credentials(configuration.Integrations.GitHub.AccessToken);
                }

                pipeline = pipeline.AddTask(new GitHubLinkTask(gitRepository, githubClient));
            }

            return pipeline;
        }
    }
}
