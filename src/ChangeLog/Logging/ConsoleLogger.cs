using System;
using Microsoft.Extensions.Logging;

namespace Grynwald.ChangeLog.Logging
{
    internal sealed class ConsoleLogger : ILogger
    {
        private static readonly object s_ConsoleLock = new object();

        private readonly LoggerOptions m_LoggerOptions;
        private readonly string? m_CategoryName;

        private class LoggerScope : IDisposable
        {
            public void Dispose()
            { }
        }


        public ConsoleLogger(LoggerOptions loggerOptions, string categoryName)
        {
            m_CategoryName = String.IsNullOrEmpty(categoryName) ? null : categoryName;
            m_LoggerOptions = loggerOptions ?? throw new ArgumentNullException(nameof(loggerOptions));
        }


        public IDisposable BeginScope<TState>(TState state) => new LoggerScope();

        public bool IsEnabled(LogLevel logLevel) => logLevel >= m_LoggerOptions.MinimumLogLevel;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var color = GetConsoleColor(logLevel);

            var message = (!m_LoggerOptions.ShowCategoryName || m_CategoryName == null)
                ? $"{logLevel.ToString().ToUpper()} - {formatter(state, exception)}"
                : $"{logLevel.ToString().ToUpper()} - {m_CategoryName} - {formatter(state, exception)}";

            lock (s_ConsoleLock)
            {
                if (color.HasValue)
                {
                    var previousColor = Console.ForegroundColor;
                    Console.ForegroundColor = color.Value;
                    Console.WriteLine(message);
                    Console.ForegroundColor = previousColor;
                }
                else
                {
                    Console.WriteLine(message);
                }
            }
        }


        private static ConsoleColor? GetConsoleColor(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Information:
                    return ConsoleColor.White;

                case LogLevel.Warning:
                    return ConsoleColor.Yellow;

                case LogLevel.Error:
                case LogLevel.Critical:
                    return ConsoleColor.Red;

                default:
                    return null;
            }
        }
    }
}
