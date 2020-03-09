using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ChangeLogCreator.Configuration;
using Xunit;

namespace ChangeLogCreator.Test
{
    public class CommandLineParametersTest
    {
        public static IEnumerable<object[]> Properties()
        {
            foreach (var property in typeof(CommandLineParameters).GetProperties())
            {
                yield return new[] { property.Name };
            }
        }

        [Theory]
        [MemberData(nameof(Properties))]
        public void Properties_have_a_configuration_value_attribute(string propertyName)
        {
            // all properties inCommandLineParameters should have a ConfigurationValueAttribute
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
    }
}
