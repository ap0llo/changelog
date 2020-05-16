using System.Collections.Generic;
using Grynwald.Utilities.Collections;
using Microsoft.Extensions.Logging;

namespace Grynwald.ChangeLog.Logging
{
    internal sealed class ConsoleLoggerProvider : ILoggerProvider
    {
        private readonly IDictionary<string, ILogger> m_Loggers = new Dictionary<string, ILogger>();
        private readonly LoggerOptions m_LoggerOptions;

        public ConsoleLoggerProvider(LoggerOptions loggerOptions)
        {
            m_LoggerOptions = loggerOptions;
        }

        public ILogger CreateLogger(string categoryName) => m_Loggers.GetOrAdd(categoryName, () => new ConsoleLogger(m_LoggerOptions, categoryName));

        public void Dispose() => m_Loggers.Clear();
    }
}
