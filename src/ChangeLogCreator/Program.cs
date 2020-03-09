using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using ChangeLogCreator.Configuration;
using ChangeLogCreator.Git;
using ChangeLogCreator.Integrations;
using ChangeLogCreator.Tasks;
using CommandLine;

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

            var configuration = ChangeLogConfigurationLoader.GetConfiguation(commandlineParameters.RepositoryPath, commandlineParameters);
            using (var gitRepository = new GitRepository(configuration.RepositoryPath))
            {    
                var containerBuilder = new ContainerBuilder();

                containerBuilder.RegisterInstance(configuration).SingleInstance();
                containerBuilder.RegisterInstance(gitRepository).SingleInstance().As<IGitRepository>();

                containerBuilder.RegisterType<LoadVersionsTask>();
                containerBuilder.RegisterType<ParseCommitsTask>();
                containerBuilder.RegisterType<RenderMarkdownTask>();

                containerBuilder.RegisterIntegrations();

                using (var container = containerBuilder.Build())
                {
                    var pipeline = new ChangeLogPipelineBuilder(container)
                        .AddTask<LoadVersionsTask>()
                        .AddTask<ParseCommitsTask>()
                        .AddIntegrationTasks()
                        .AddTask<RenderMarkdownTask>()
                        .Build();

                    await pipeline.RunAsync();
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
