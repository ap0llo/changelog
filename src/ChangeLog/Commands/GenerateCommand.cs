using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Autofac;
using FluentValidation;
using Grynwald.ChangeLog.CommandLine;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.Filtering;
using Grynwald.ChangeLog.Git;
using Grynwald.ChangeLog.Integrations;
using Grynwald.ChangeLog.Logging;
using Grynwald.ChangeLog.Pipeline;
using Grynwald.ChangeLog.Tasks;
using Grynwald.ChangeLog.Templates;
using Grynwald.Utilities.Configuration;
using Microsoft.Extensions.Logging;


namespace Grynwald.ChangeLog.Commands
{
    internal class GenerateCommand : ICommand
    {
        /// <summary>
        /// Collects settings that are determined dynamically (in <see cref="RunAsync()"/>
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


        private readonly GenerateCommandLineParameters m_CommandLineParameters;
        private readonly ILogger<GenerateCommand> m_Logger;
        private readonly IValidator<GenerateCommandLineParameters> m_CommandLineValidator;
        private readonly IValidator<ChangeLogConfiguration> m_ConfigurationValidator;

        public GenerateCommand(GenerateCommandLineParameters commandLine, ILogger<GenerateCommand> logger, IValidator<GenerateCommandLineParameters> commandLineValidator, IValidator<ChangeLogConfiguration> configurationValidator)
        {
            m_CommandLineParameters = commandLine ?? throw new ArgumentNullException(nameof(commandLine));
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            m_CommandLineValidator = commandLineValidator ?? throw new ArgumentNullException(nameof(commandLineValidator));
            m_ConfigurationValidator = configurationValidator ?? throw new ArgumentNullException(nameof(configurationValidator));
        }



        public async Task<int> RunAsync()
        {
            if (!ValidateCommandlineParameters(m_CommandLineParameters))
                return 1;

            if (!TryGetRepositoryPath(m_CommandLineParameters, out var repositoryPath))
                return 1;

            if (!TryLoadConfiguration(repositoryPath, out var configuration))
                return 1;

            if (!TryOpenRepository(repositoryPath, out var gitRepository))
                return 1;

            using (gitRepository)
            {
                var containerBuilder = new ContainerBuilder();

                containerBuilder.RegisterInstance(configuration).SingleInstance();
                containerBuilder.RegisterInstance(gitRepository).SingleInstance().As<IGitRepository>();

                containerBuilder.RegisterLogging(CompositionRoot.CreateLoggerConfiguration(m_CommandLineParameters.Verbose));

                containerBuilder.RegisterType<ChangeLogPipeline>();

                containerBuilder.RegisterType<LoadCurrentVersionTask>().As<IChangeLogTask>();
                containerBuilder.RegisterType<LoadVersionsFromTagsTask>().As<IChangeLogTask>();
                containerBuilder.RegisterType<LoadCommitsTask>().As<IChangeLogTask>();
                containerBuilder.RegisterType<LoadMessageOverridesFromGitNotesTask>().As<IChangeLogTask>();
                containerBuilder.RegisterType<LoadMessageOverridesFromFileSystemTask>().As<IChangeLogTask>();
                containerBuilder.RegisterType<ParseCommitsTask>().As<IChangeLogTask>();
                containerBuilder.RegisterType<ParseCommitReferencesTask>().As<IChangeLogTask>();
                containerBuilder.RegisterType<FilterVersionsTask>().As<IChangeLogTask>();
                containerBuilder.RegisterType<FilterEntriesTask>().As<IChangeLogTask>();
                containerBuilder.RegisterType<ResolveEntryReferencesTask>().As<IChangeLogTask>();
                containerBuilder.RegisterType<AddCommitFooterTask>().As<IChangeLogTask>();
                containerBuilder.RegisterType<ParseWebLinksTask>().As<IChangeLogTask>();
                containerBuilder.RegisterType<RenderTemplateTask>().As<IChangeLogTask>();

                containerBuilder.RegisterIntegrations();

                containerBuilder.RegisterTemplates();

                using (var container = containerBuilder.Build())
                {
                    var pipeline = container.Resolve<ChangeLogPipeline>();

                    var result = await pipeline.RunAsync();
                    return result.Success ? 0 : 1;
                }
            }
        }


        private static string? GetConfigurationFilePath(GenerateCommandLineParameters commandlineParameters, string repositoryPath)
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

        private bool ValidateCommandlineParameters(GenerateCommandLineParameters parameters)
        {
            var result = m_CommandLineValidator.Validate(parameters);

            foreach (var error in result.Errors)
            {
                m_Logger.LogError(error.ToString());
            }

            return result.IsValid;
        }

        private bool TryGetRepositoryPath(GenerateCommandLineParameters parameters, [NotNullWhen(true)] out string? repositoryPath)
        {
            if (!String.IsNullOrEmpty(parameters.RepositoryPath))
            {
                repositoryPath = Path.GetFullPath(parameters.RepositoryPath);
                return true;
            }

            if (RepositoryLocator.TryGetRepositoryPath(Environment.CurrentDirectory, out repositoryPath))
            {
                m_Logger.LogInformation($"Found git repository at '{repositoryPath}'");
                return true;
            }
            else
            {
                m_Logger.LogError($"No git repository found in '{Environment.CurrentDirectory}' or any of its parent directories");
                repositoryPath = default;
                return false;
            }
        }

        private bool TryOpenRepository(string repositoryPath, [NotNullWhen(true)] out IGitRepository? repository)
        {
            try
            {
                repository = new GitRepository(repositoryPath);
                return true;
            }
            catch (RepositoryNotFoundException ex)
            {
                m_Logger.LogDebug(ex, $"Failed to open repository at '{repositoryPath}'");
                m_Logger.LogError($"'{repositoryPath}' is not a git repository");
                repository = default;
                return false;
            }
        }

        private bool TryLoadConfiguration(string repositoryPath, [NotNullWhen(true)] out ChangeLogConfiguration? configuration)
        {
            var configurationFilePath = GetConfigurationFilePath(m_CommandLineParameters, repositoryPath);
            if (File.Exists(configurationFilePath))
                m_Logger.LogDebug($"Using configuration file '{configurationFilePath}'");
            else
                m_Logger.LogDebug("Continuing without loading a configuration file, because no configuration file was wound");


            // pass repository path to configuration loader to make it available through the configuration system
            var dynamicSettings = new DynamicallyDeterminedSettings()
            {
                RepositoryPath = repositoryPath
            };

            configuration = ChangeLogConfigurationLoader.GetConfiguration(configurationFilePath, m_CommandLineParameters, dynamicSettings);

            var validationResult = m_ConfigurationValidator.Validate(configuration);

            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    m_Logger.LogError($"Invalid configuration: {error.ErrorMessage}");
                }

                m_Logger.LogError($"Validation of configuration failed");
                return false;
            }

            return true;

        }
    }
}
