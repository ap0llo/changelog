using Microsoft.Extensions.Logging;

namespace Grynwald.ChangeLog.Logging
{
    internal sealed class LoggerOptions
    {
        public LogLevel MinimumLogLevel { get; }

        public bool ShowCategoryName { get; }


        public LoggerOptions(LogLevel minimumLogLevel, bool showCategoryName)
        {
            MinimumLogLevel = minimumLogLevel;
            ShowCategoryName = showCategoryName;
        }
    }
}
