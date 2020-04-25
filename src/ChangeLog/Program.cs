﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using CommandLine;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.Git;
using Grynwald.ChangeLog.Integrations;
using Grynwald.ChangeLog.Logging;
using Grynwald.ChangeLog.Tasks;
using Grynwald.ChangeLog.Templates;
using Microsoft.Extensions.Logging;

namespace Grynwald.ChangeLog
{
    internal static class Program
    {
        private const string s_DefaultConfigurationFileName = "changelog.settings.json";


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
            // for validation of command line parameters, directly create a console logger
            // bypassing the DI container because we need to validate the parameters
            // before setting up DI
            var logger = new ConsoleLogger(LogLevel.Information, "");

            if (!ValidateCommandlineParameters(commandlineParameters, logger))
                return 1;

            var configurationFilePath = !String.IsNullOrEmpty(commandlineParameters.ConfigurationFilePath)
                ? commandlineParameters.ConfigurationFilePath
                : Path.Combine(commandlineParameters.RepositoryPath, s_DefaultConfigurationFileName);

            var configuration = ChangeLogConfigurationLoader.GetConfiguration(configurationFilePath, commandlineParameters);
            using (var gitRepository = new GitRepository(configuration.RepositoryPath))
            {
                var containerBuilder = new ContainerBuilder();

                containerBuilder.RegisterInstance(configuration).SingleInstance();
                containerBuilder.RegisterInstance(gitRepository).SingleInstance().As<IGitRepository>();

                containerBuilder.RegisterLogging(minimumLogLevel: commandlineParameters.Verbose ? LogLevel.Debug : LogLevel.Information);

                containerBuilder.RegisterType<ChangeLogPipeline>();

                containerBuilder.RegisterType<LoadCurrentVersionTask>();
                containerBuilder.RegisterType<LoadVersionsFromTagsTask>();
                containerBuilder.RegisterType<ParseCommitsTask>();
                containerBuilder.RegisterType<FilterVersionsTask>();
                containerBuilder.RegisterType<RenderTemplateTask>();

                containerBuilder.RegisterIntegrations();

                try
                {
                    containerBuilder.RegisterTemplate(configuration.Template);
                }
                catch (InvalidTemplateConfigurationException ex)
                {
                    logger.LogCritical($"Failed to load template: {ex.Message}");
                    return 1;
                }

                using (var container = containerBuilder.Build())
                {
                    // Note: The order of the tasks added here is important.
                    // E.g. In order for commits for versions loaded correctly, ParseCommitsTask needs to run before FilterVersionsTask
                    var pipeline = new ChangeLogPipelineBuilder(container)
                        .AddTask<LoadCurrentVersionTask>()
                        .AddTask<LoadVersionsFromTagsTask>()
                        .AddTask<ParseCommitsTask>()
                        .AddTask<FilterVersionsTask>()
                        .AddIntegrationTasks()
                        .AddTask<RenderTemplateTask>()
                        .Build();

                    var result = await pipeline.RunAsync();
                    return result.Success ? 0 : 1;
                }
            }
        }

        private static bool ValidateCommandlineParameters(CommandLineParameters parameters, ILogger logger)
        {
            var validator = new CommandLineParametersValidator();
            var result = validator.Validate(parameters);

            foreach (var error in result.Errors)
            {
                logger.LogError(error.ToString());
            }

            return result.IsValid;
        }
    }
}
