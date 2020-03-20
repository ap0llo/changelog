using System.Collections.Generic;
using Grynwald.Utilities.Collections;
using Microsoft.Extensions.Logging;

namespace Grynwald.ChangeLog.Logging
{
    internal sealed class ConsoleLoggerProvider : ILoggerProvider
    {
        private readonly IDictionary<string, ILogger> m_Loggers = new Dictionary<string, ILogger>();
        private readonly LogLevel m_MinimumLogLevel;

        public ConsoleLoggerProvider(LogLevel minimumLogLevel)
        {
            m_MinimumLogLevel = minimumLogLevel;
        }

        public ILogger CreateLogger(string categoryName) => m_Loggers.GetOrAdd(categoryName, () => new ConsoleLogger(m_MinimumLogLevel, categoryName));

        public void Dispose() => m_Loggers.Clear();
    }
}
