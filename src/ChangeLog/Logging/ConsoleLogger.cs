using System;
using Microsoft.Extensions.Logging;

namespace Grynwald.ChangeLog.Logging
{
    internal sealed class ConsoleLogger : ILogger
    {
        private static readonly object s_ConsoleLock = new object();

        private readonly LogLevel m_MinimumLogLevel;
        private readonly string? m_CategoryName;

        private class LoggerScope : IDisposable
        {
            public void Dispose()
            { }
        }


        public ConsoleLogger(LogLevel minimumLogLevel, string categoryName)
        {
            m_MinimumLogLevel = minimumLogLevel;
            m_CategoryName = String.IsNullOrEmpty(categoryName) ? null : categoryName;
        }


        public IDisposable BeginScope<TState>(TState state) => new LoggerScope();

        public bool IsEnabled(LogLevel logLevel) => logLevel >= m_MinimumLogLevel;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var color = GetConsoleColor(logLevel);

            var message = m_CategoryName == null
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
