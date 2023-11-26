using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CommandLine;
using Grynwald.ChangeLog.CommandLine;
using Grynwald.Utilities.Configuration;
using Xunit;

namespace Grynwald.ChangeLog.Test.CommandLine
{
    /// <summary>
    /// Tests for <see cref="GenerateCommandLineParameters"/>
    /// </summary>
    public class GenerateCommandLineParametersTest
    {
        public static IEnumerable<object[]> Properties() =>
            typeof(GenerateCommandLineParameters).GetProperties().Select(p => new[] { p.Name });

        [Theory]
        [MemberData(nameof(Properties))]
        public void Properties_have_a_configuration_value_attribute(string propertyName)
        {
            // the "--verbose", "--configurationPath"  and "--repositoryPath"
            // switches have no corresponding configuration setting and are
            // processed before the configuration system is initialized

            if (propertyName == nameof(GenerateCommandLineParameters.Verbose) ||
                propertyName == nameof(GenerateCommandLineParameters.ConfigurationFilePath) ||
                propertyName == nameof(GenerateCommandLineParameters.RepositoryPath))
            {
                return;
            }

            // All other properties in CommandLineParameters should have a ConfigurationValue attribute
            // so the value can be used in the configuration system.

            var property = typeof(GenerateCommandLineParameters).GetProperty(propertyName)!;

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
            var sut = new GenerateCommandLineParameters()
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
        public void CommandLineParameters_returns_null_if_output_path_is_null_or_empty(string? outputPath)
        {
            // ARRANGE
            var sut = new GenerateCommandLineParameters()
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
            // All properties in CommandLineParameters should have a OptionAttribute so the value can be set by the commandline parser

            var property = typeof(GenerateCommandLineParameters).GetProperty(propertyName)!;

            var attribute = property.GetCustomAttribute<OptionAttribute>();

            Assert.NotNull(attribute);
        }

        [Theory]
        [MemberData(nameof(Properties))]
        public void Properties_with_a_Option_or_Value_attribute_have_a_getter_and_a_setter(string propertyName)
        {
            var property = typeof(GenerateCommandLineParameters).GetProperty(propertyName)!;

            if (property.GetCustomAttribute<OptionAttribute>() is not null || property.GetCustomAttribute<ValueAttribute>() is not null)
            {
                Assert.NotNull(property.GetGetMethod());
                Assert.NotNull(property.GetSetMethod());
            }
        }
    }
}
