using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CommandLine;
using Grynwald.ChangeLog.Configuration;
using Grynwald.Utilities.Configuration;
using Xunit;

namespace Grynwald.ChangeLog.Test
{
    /// <summary>
    /// Tests for <see cref="CommandLineParameters"/>
    /// </summary>
    public class CommandLineParametersTest
    {
        public static IEnumerable<object[]> Properties() =>
            typeof(CommandLineParameters).GetProperties().Select(p => new[] { p.Name });

        [Theory]
        [MemberData(nameof(Properties))]
        public void Properties_have_a_configuration_value_attribute(string propertyName)
        {
            // the "--verbose", "--configurationPath"  and "--repositoryPath"
            // switches haveno corresponding configuration setting and are
            // processed before the configuration system is initialized

            if (propertyName == nameof(CommandLineParameters.Verbose) ||
                propertyName == nameof(CommandLineParameters.ConfigurationFilePath) ||
                propertyName == nameof(CommandLineParameters.RepositoryPath))
            {
                return;
            }

            // All other properties in CommandLineParameters should have a ConfigurationValue attribute
            // so the value can be used in the configuration system.

            var property = typeof(CommandLineParameters).GetProperty(propertyName)!;

            var attribute = property.GetCustomAttribute<ConfigurationValueAttribute>();

            Assert.NotNull(attribute);
            Assert.NotNull(attribute!.Key);
            Assert.StartsWith("changelog:", attribute!.Key);
            Assert.NotEqual("changelog:", attribute!.Key);
        }

        [Fact]
        public void CommandLineParameters_returns_a_rooted_output_path()
        {
            // ARRANGE
            var sut = new CommandLineParameters()
            {
                OutputPath = "somePath"
            };

            // ACT 
            var outputPath = sut.OutputPath;

            // ASSERT
            Assert.Equal(Path.Combine(Environment.CurrentDirectory, "somePath"), outputPath);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void CommandLineParameters_returns_null_is_output_path_is_null_or_empty(string outputPath)
        {
            // ARRANGE
            var sut = new CommandLineParameters()
            {
                OutputPath = outputPath
            };

            // ACT / ASSEERT
            Assert.Null(sut.OutputPath);
        }


        [Theory]
        [MemberData(nameof(Properties))]
        public void Properties_have_a_Option_attribute(string propertyName)
        {
            // all properties inCommandLineParameters should have a OptionAttribute
            // so the value can be set by the commandline parser

            var property = typeof(CommandLineParameters).GetProperty(propertyName)!;

            var attribute = property.GetCustomAttribute<OptionAttribute>();

            Assert.NotNull(attribute);
        }

        [Theory]
        [MemberData(nameof(Properties))]
        public void Properties_with_a_Option_or_Value_attribute_have_a_getter_and_a_setter(string propertyName)
        {
            var property = typeof(CommandLineParameters).GetProperty(propertyName)!;

            if (property.GetCustomAttribute<OptionAttribute>() != null || property.GetCustomAttribute<ValueAttribute>() != null)
            {
                Assert.NotNull(property.GetGetMethod());
                Assert.NotNull(property.GetSetMethod());
            }
        }

        [Fact]
        public void Template_parameter_is_optional()
        {
            // ARRANGE
            var args = new[] { "--repository", "some-path" };

            // ACT
            var result = CommandLineParameters.Parse(args);

            // ASSERT
            Assert.Equal(ParserResultType.Parsed, result.Tag);
            Assert.Equal(typeof(CommandLineParameters), result.TypeInfo.Current);
            result.WithParsed(parsed =>
            {
                Assert.Null(parsed.Template);
            });
        }

        public static IEnumerable<object[]> TemplateNames()
        {
            foreach (var value in Enum.GetValues(typeof(ChangeLogConfiguration.TemplateName)).Cast<ChangeLogConfiguration.TemplateName>())
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
            var result = CommandLineParameters.Parse(args);

            // ASSERT
            Assert.Equal(ParserResultType.Parsed, result.Tag);
            Assert.Equal(typeof(CommandLineParameters), result.TypeInfo.Current);
            result.WithParsed(parsed =>
            {
                Assert.NotNull(parsed.Template);
                Assert.Equal(expected, parsed.Template);
            });
        }
    }
}
