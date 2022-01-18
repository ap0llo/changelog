using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using CommandLine;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.Filtering;
using Grynwald.ChangeLog.Git;
using Grynwald.ChangeLog.Integrations;
using Grynwald.ChangeLog.Logging;
using Grynwald.ChangeLog.Pipeline;
using Grynwald.ChangeLog.Tasks;
using Grynwald.ChangeLog.Templates;
using Grynwald.Utilities.Configuration;
using Grynwald.Utilities.Logging;
using Microsoft.Extensions.Logging;

namespace Grynwald.ChangeLog
{
    internal static class Program
    {
        /// <summary>
        /// Collects settings that are determined dynamically (in <see cref="RunAsync(CommandLineParameters)"/>
        /// that need to be passed into the configuration system.
        /// </summary>
        private class DynamicallyDeterminedSettings
        {
            [ConfigurationValue("changelog:repositoryPath")]
            public string RepositoryPath { get; set; } = "";
        }


        private static readonly string[] s_DefaultConfigurationFilePaths =
        {
            "changelog.settings.json",
            ".config/changelog/settings.json"
        };


        private static async Task<int> Main(string[] args)
        {
            return await CommandLineParameters.Parse(args)
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
            var loggerOptions = commandlineParameters.Verbose
                ? new SimpleConsoleLoggerConfiguration(LogLevel.Debug, true, true)
                : new SimpleConsoleLoggerConfiguration(LogLevel.Information, false, true);

            // for validation of command line parameters, directly create a console logger
            // bypassing the DI container because we need to validate the parameters
            // before setting up DI
            var logger = new SimpleConsoleLogger(loggerOptions, "");

            if (!ValidateCommandlineParameters(commandlineParameters, logger))
                return 1;

            if (!TryGetRepositoryPath(commandlineParameters, logger, out var repositoryPath))
                return 1;

            if (!TryOpenRepository(repositoryPath, logger, out var gitRepository))
                return 1;

            var configurationFilePath = GetConfigurationFilePath(commandlineParameters, repositoryPath);
            if (File.Exists(configurationFilePath))
                logger.LogDebug($"Using configuration file '{configurationFilePath}'");
            else
                logger.LogDebug("Continuing without loading a configuration file, because no configuration file was wound");


            // pass repository path to configuration loader to make it available through the configuration system
            var dynamicSettings = new DynamicallyDeterminedSettings()
            {
                RepositoryPath = repositoryPath
            };

            var configuration = ChangeLogConfigurationLoader.GetConfiguration(configurationFilePath, commandlineParameters, dynamicSettings);

            using (gitRepository)
            {
                var containerBuilder = new ContainerBuilder();

                containerBuilder.RegisterType<ConfigurationValidator>();
                containerBuilder.RegisterInstance(configuration).SingleInstance();
                containerBuilder.RegisterInstance(gitRepository).SingleInstance().As<IGitRepository>();

                containerBuilder.RegisterLogging(loggerOptions);

                containerBuilder.RegisterType<ChangeLogPipeline>();

                containerBuilder.RegisterType<LoadCurrentVersionTask>();
                containerBuilder.RegisterType<LoadVersionsFromTagsTask>();
                containerBuilder.RegisterType<ParseCommitsTask>();
                containerBuilder.RegisterType<ParseCommitReferencesTask>();
                containerBuilder.RegisterType<FilterVersionsTask>();
                containerBuilder.RegisterType<FilterEntriesTask>();
                containerBuilder.RegisterType<ResolveEntryReferencesTask>();
                containerBuilder.RegisterType<AddCommitFooterTask>();
                containerBuilder.RegisterType<ParseWebLinksTask>();
                containerBuilder.RegisterType<RenderTemplateTask>();

                containerBuilder.RegisterIntegrations();

                try
                {
                    containerBuilder.RegisterTemplate(configuration.Template);
                }
                catch (InvalidTemplateConfigurationException ex)
                {
                    logger.LogError($"Failed to load template: {ex.Message}");
                    return 1;
                }

                using (var container = containerBuilder.Build())
                {
                    var configurationValidator = container.Resolve<ConfigurationValidator>();
                    var validationResult = configurationValidator.Validate(configuration);

                    if (!validationResult.IsValid)
                    {
                        foreach (var error in validationResult.Errors)
                        {
                            logger.LogError($"Invalid configuration: {error.ErrorMessage}");
                        }

                        logger.LogError($"Validation of configuration failed");
                        return 1;
                    }

                    var pipeline = new ChangeLogPipelineBuilder(container)
                        .AddTask<LoadCurrentVersionTask>()
                        .AddTask<LoadVersionsFromTagsTask>()
                        .AddTask<ParseCommitsTask>()
                        .AddTask<ParseCommitReferencesTask>()
                        .AddTask<ParseWebLinksTask>()
                        .AddTask<FilterVersionsTask>()
                        .AddTask<FilterEntriesTask>()
                        .AddTask<ResolveEntryReferencesTask>()
                        .AddTask<AddCommitFooterTask>()
                        .AddIntegrationTasks()
                        .AddTask<RenderTemplateTask>()
                        .Build();

                    var result = await pipeline.RunAsync();
                    return result.Success ? 0 : 1;
                }
            }
        }

        private static string? GetConfigurationFilePath(CommandLineParameters commandlineParameters, string repositoryPath)
        {
            if (!String.IsNullOrEmpty(commandlineParameters.ConfigurationFilePath))
                return commandlineParameters.ConfigurationFilePath;

            foreach (var path in s_DefaultConfigurationFilePaths)
            {
                var absolutePath = Path.GetFullPath(Path.Combine(repositoryPath, path));

                if (File.Exists(absolutePath))
                    return absolutePath;
            }

            return null;
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

        private static bool TryGetRepositoryPath(CommandLineParameters parameters, ILogger logger, [NotNullWhen(true)] out string? repositoryPath)
        {
            if (!String.IsNullOrEmpty(parameters.RepositoryPath))
            {
                repositoryPath = Path.GetFullPath(parameters.RepositoryPath);
                return true;
            }

            if (RepositoryLocator.TryGetRepositoryPath(Environment.CurrentDirectory, out repositoryPath))
            {
                logger.LogInformation($"Found git repository at '{repositoryPath}'");
                return true;
            }
            else
            {
                logger.LogError($"No git repository found in '{Environment.CurrentDirectory}' or any of its parent directories");
                repositoryPath = default;
                return false;
            }
        }

        private static bool TryOpenRepository(string repositoryPath, ILogger logger, [NotNullWhen(true)] out IGitRepository? repository)
        {
            try
            {
                repository = new GitRepository(repositoryPath);
                return true;
            }
            catch (RepositoryNotFoundException ex)
            {
                logger.LogDebug(ex, $"Failed to open repository at '{repositoryPath}'");
                logger.LogError($"'{repositoryPath}' is not a git repository");
                repository = default;
                return false;
            }
        }
    }
}
