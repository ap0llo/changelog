using System;
using System.Collections.Generic;
using Grynwald.ChangeLog.CommandLine;
using Grynwald.ChangeLog.Configuration;
using Xunit;

namespace Grynwald.ChangeLog.Test.CommandLine
{

    /// <summary>
    /// Tests for <see cref="CommandLineParser"/>
    /// </summary>
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
            var parsed = CommandLineAssert.Parsed<GenerateCommandLineParameters>(result);
            Assert.Null(parsed.Template!);
        }

        public static IEnumerable<object[]> TemplateNames()
        {
            foreach (var value in Enum.GetValues<ChangeLogConfiguration.TemplateName>())
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
            var parsed = CommandLineAssert.Parsed<GenerateCommandLineParameters>(result);
            Assert.Equal(expected, parsed.Template);
        }


        [Theory]
        [InlineData(new object[] { new string[] { } })]
        [InlineData(new object[] { new string[] { "--repository", "some-path" } })]
        public void The_generate_command_is_the_default_command(string[] args)
        {
            // ARRANGE

            // ACT
            var result = CommandLineParser.Parse(args);

            // ASSERT
            CommandLineAssert.Parsed<GenerateCommandLineParameters>(result);
        }

        [Theory]
        [InlineData(new object[] { new string[] { "generate" } })]
        [InlineData(new object[] { new string[] { "generate", "--repository", "some-path" } })]
        [InlineData(new object[] { new string[] { "g" } })]
        [InlineData(new object[] { new string[] { "g", "--repository", "some-path" } })]
        public void The_generate_command_is_parsed_correctly(string[] args)
        {
            // ARRANGE

            // ACT
            var result = CommandLineParser.Parse(args);

            // ASSERT
            CommandLineAssert.Parsed<GenerateCommandLineParameters>(result);
        }

    }
}
