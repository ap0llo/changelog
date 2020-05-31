using Autofac;
using Microsoft.Extensions.Logging;
using Grynwald.Utilities.Logging;

namespace Grynwald.ChangeLog.Logging
{
    internal static class ContainerBuilderExtensions
    {
        public static void RegisterLogging(this ContainerBuilder builder, SimpleConsoleLoggerConfiguration loggerConfiguration)
        {
            var loggerFactory = new LoggerFactory();
            var provider = new SimpleConsoleLoggerProvider(loggerConfiguration);
            loggerFactory.AddProvider(provider);

            builder.RegisterInstance(loggerFactory).As<ILoggerFactory>().SingleInstance();
            builder.RegisterGeneric(typeof(Logger<>)).As(typeof(ILogger<>)).SingleInstance();
        }
    }
}
