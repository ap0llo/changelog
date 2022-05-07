using System;
using CommandLine;

namespace Grynwald.ChangeLog.CommandLine
{
    internal static class CommandLineParser
    {
        public static ParserResult<CommandLineParameters> Parse(string[] args)
        {
            using var commandlineParser = new Parser(settings =>
            {
                settings.CaseInsensitiveEnumValues = true;
                settings.CaseSensitive = false;
                settings.HelpWriter = Console.Out;
            });

            return commandlineParser.ParseArguments<CommandLineParameters>(args);
        }
    }
}
