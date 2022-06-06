using System;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Grynwald.ChangeLog.Test
{
    public class XunitLogger : TestLogger
    {
        private readonly ITestOutputHelper m_TestOutputHelper;
        private readonly string? m_CategoryName;

        private class LoggerScope : IDisposable
        {
            public void Dispose()
            { }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="XunitLogger"/>
        /// </summary>
        public XunitLogger(ITestOutputHelper testOutputHelper, string? categoryName) : base(categoryName)
        {
            m_TestOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
            m_CategoryName = String.IsNullOrEmpty(categoryName) ? null : categoryName;
        }


        /// <inheritdoc />
        public override void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            base.Log(logLevel, eventId, state, exception, formatter);

            if (IsEnabled(logLevel))
            {
                m_TestOutputHelper.WriteLine($"{logLevel.ToString().ToUpper()} - {m_CategoryName} - {formatter(state, exception)}");
            }
        }
    }


    public class XunitLogger<T> : XunitLogger, ILogger<T>
    {
        public XunitLogger(ITestOutputHelper testOutputHelper) : base(testOutputHelper, typeof(T).Name)
        { }
    }
}
