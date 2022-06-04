using CommandLine;
using Xunit;

namespace Grynwald.ChangeLog.Test.CommandLine
{
    internal static class CommandLineAssert
    {
        public static T Parsed<T>(ParserResult<object> parserResult)
        {
            Assert.Equal(ParserResultType.Parsed, parserResult.Tag);
            var value = Assert.IsType<T>(parserResult.Value);
            return value;
        }
    }
}
