using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Grynwald.ChangeLog.Test
{
    public class TestLogger : ILogger
    {
        private readonly string? m_CategoryName;
        private readonly List<string> m_LoggedMessages = new();

        private class LoggerScope : IDisposable
        {
            public void Dispose()
            { }
        }

        public IReadOnlyList<string> LoggedMessages => m_LoggedMessages;

        /// <summary>
        /// Initializes a new instance of <see cref="TestLogger"/>
        /// </summary>
        public TestLogger(string? categoryName)
        {
            m_CategoryName = String.IsNullOrEmpty(categoryName) ? null : categoryName;
        }

        /// <inheritdoc />
        public IDisposable BeginScope<TState>(TState state) => new LoggerScope();

        /// <inheritdoc />
        public bool IsEnabled(LogLevel logLevel) => true;

        /// <inheritdoc />
        public virtual void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (IsEnabled(logLevel))
            {
                m_LoggedMessages.Add($"{logLevel.ToString().ToUpper()} - {m_CategoryName} - {formatter(state, exception)}");
            }
        }
    }


    public class TestLogger<T> : TestLogger, ILogger<T>
    {
        public TestLogger() : base(typeof(T).Name)
        { }
    }
}
