using System;
using System.Collections.Generic;
using System.Linq;
using Grynwald.ChangeLog.Configuration;
using Xunit;

namespace Grynwald.ChangeLog.Test
{
    public class CommandLineParserTest
    {
        [Fact]
        public void Template_parameter_is_optional()
        {
            // ARRANGE
            var args = new[] { "--repository", "some-path" };

            // ACT
            var result = CommandLineParser.Parse(args);

            // ASSERT
            var parsed = CommandLineAssert.Parsed(result);
            Assert.Null(parsed.Template!);
        }

        public static IEnumerable<object[]> TemplateNames()
        {
#if NETCOREAPP3_1
            foreach (var value in Enum.GetValues(typeof(ChangeLogConfiguration.TemplateName)).Cast<ChangeLogConfiguration.TemplateName>())
#else
            foreach (var value in Enum.GetValues<ChangeLogConfiguration.TemplateName>())
#endif
            {
                yield return new object[] { value.ToString(), value };
                yield return new object[] { value.ToString().ToLower(), value };
                yield return new object[] { value.ToString().ToUpper(), value };
            }
        }

        [Theory]
        [MemberData(nameof(TemplateNames))]
        public void Template_parameter_is_parsed_correctly(string template, ChangeLogConfiguration.TemplateName expected)
        {
            // ARRANGE
            var args = new[] { "--repository", "some-path", "--template", template };

            // ACT
            var result = CommandLineParser.Parse(args);

            // ASSERT
            var parsed = CommandLineAssert.Parsed(result);
            Assert.Equal(expected, parsed.Template);
        }
    }
}
