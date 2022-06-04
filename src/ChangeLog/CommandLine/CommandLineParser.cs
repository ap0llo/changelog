using System;
using CommandLine;

namespace Grynwald.ChangeLog.CommandLine
{
    internal static class CommandLineParser
    {
        public static ParserResult<object> Parse(string[] args)
        {
            using var commandlineParser = new Parser(settings =>
            {
                settings.CaseInsensitiveEnumValues = true;
                settings.CaseSensitive = false;
                settings.HelpWriter = Console.Out;
            });

            return commandlineParser.ParseArguments<GenerateCommandLineParameters, DummyCommandLineParameters>(args);
        }
    }
}
