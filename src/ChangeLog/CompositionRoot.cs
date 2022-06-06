using System;
using Grynwald.ChangeLog.CommandLine;
using Grynwald.ChangeLog.Commands;
using Grynwald.ChangeLog.Configuration;
using Grynwald.Utilities.Logging;
using Microsoft.Extensions.Logging;

namespace Grynwald.ChangeLog
{
    internal class CompositionRoot : IDisposable
    {

        public GenerateCommand CreateGenerateCommand(GenerateCommandLineParameters commandLine)
        {
            if (commandLine is null)
                throw new ArgumentNullException(nameof(commandLine));

            return new GenerateCommand(
                commandLine: commandLine,
                logger: CreateLogger<GenerateCommand>(commandLine.Verbose),
                commandLineValidator: new GenerateCommandLineParametersValidator(),
                configurationValidator: new ConfigurationValidator()
            );
        }



        public ILogger<T> CreateLogger<T>(bool verbose)
        {
            var loggerFactory = new LoggerFactory();
            var provider = new SimpleConsoleLoggerProvider(CreateLoggerConfiguration(verbose));
            loggerFactory.AddProvider(provider);

            return new Logger<T>(loggerFactory);
        }


        //TODO: Temporary workaround
        public static SimpleConsoleLoggerConfiguration CreateLoggerConfiguration(bool verbose)
        {
            return verbose
                ? new SimpleConsoleLoggerConfiguration(LogLevel.Debug, true, true)
                : new SimpleConsoleLoggerConfiguration(LogLevel.Information, false, true);
        }


        public void Dispose()
        {
            //TODO
        }
    }
}
