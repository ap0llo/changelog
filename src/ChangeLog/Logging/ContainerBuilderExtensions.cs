using Autofac;
using Microsoft.Extensions.Logging;

namespace Grynwald.ChangeLog.Logging
{
    internal static class ContainerBuilderExtensions
    {
        public static void RegisterLogging(this ContainerBuilder builder, LogLevel minimumLogLevel)
        {
            var loggerFactory = new LoggerFactory();
            var consoleProvider = new ConsoleLoggerProvider(minimumLogLevel);
            loggerFactory.AddProvider(consoleProvider);

            builder.RegisterInstance(loggerFactory).As<ILoggerFactory>().SingleInstance();
            builder.RegisterGeneric(typeof(Logger<>)).As(typeof(ILogger<>)).SingleInstance();
        }
    }
}
