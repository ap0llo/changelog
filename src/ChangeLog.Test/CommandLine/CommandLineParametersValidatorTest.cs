using System.IO;
using Grynwald.ChangeLog.CommandLine;
using Grynwald.Utilities.IO;
using Xunit;

namespace Grynwald.ChangeLog.Test.CommandLine
{
    /// <summary>
    /// Tests for <see cref="CommandLineParametersValidator"/>
    /// </summary>
    public class CommandLineParametersValidatorTest
    {
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void RepositoryPath_may_be_empty(string repositoryPath)
        {
            // ARRANGE
            var parameters = new CommandLineParameters()
            {
                RepositoryPath = repositoryPath
            };

            var validator = new CommandLineParametersValidator();

            // ACT 
            var result = validator.Validate(parameters);

            // ASSERT
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Theory]
        [InlineData("  ")]
        [InlineData("\t")]
        public void RepositoryPath_must__not_be_whitespace(string repositoryPath)
        {
            // ARRANGE
            var parameters = new CommandLineParameters()
            {
                RepositoryPath = repositoryPath
            };

            var validator = new CommandLineParametersValidator();

            // ACT 
            var result = validator.Validate(parameters);

            // ASSERT
            Assert.False(result.IsValid);
            var error = Assert.Single(result.Errors);
            Assert.Equal(nameof(CommandLineParameters.RepositoryPath), error.PropertyName);

            // error message should contain commandline parameter name (specified using the Option attribute) instead of the property name
            Assert.Contains("'repository'", error.ErrorMessage);
        }

        [Fact]
        public void RepositoryPath_must_exists()
        {
            // ARRANGE
            using var temporaryDirectory = new TemporaryDirectory();
            var nonExistingDirectory = Path.Combine(temporaryDirectory, "someDir");

            var parameters = new CommandLineParameters()
            {
                RepositoryPath = nonExistingDirectory
            };

            var validator = new CommandLineParametersValidator();

            // ACT 
            var result = validator.Validate(parameters);

            // ASSERT
            Assert.False(result.IsValid);
            var error = Assert.Single(result.Errors);
            Assert.Equal(nameof(CommandLineParameters.RepositoryPath), error.PropertyName);

            // error message should contain commandline parameter name (specified using the Option attribute) instead of the property name
            Assert.Contains("'repository'", error.ErrorMessage);
        }


        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void CurrentVersion_may_be_empty(string value)
        {
            // ARRANGE
            using var repositoryDirectory = new TemporaryDirectory();
            var parameters = new CommandLineParameters()
            {
                RepositoryPath = repositoryDirectory,
                CurrentVersion = value
            };

            var validator = new CommandLineParametersValidator();

            // ACT 
            var result = validator.Validate(parameters);

            // ASSERT
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Theory]
        [InlineData("not-a-version")]
        public void CurrentVersion_must_be_a_valid_version_if_parameter_is_set(string version)
        {
            // ARRANGE
            using var repositoryDirectory = new TemporaryDirectory();

            var parameters = new CommandLineParameters()
            {
                RepositoryPath = repositoryDirectory,
                CurrentVersion = version
            };

            var validator = new CommandLineParametersValidator();

            // ACT 
            var result = validator.Validate(parameters);

            // ASSERT
            Assert.False(result.IsValid);
            var error = Assert.Single(result.Errors);
            Assert.Equal(nameof(CommandLineParameters.CurrentVersion), error.PropertyName);

            // error message should contain commandline parameter name (specified using the Option attribute) instead of the property name
            Assert.Contains("'currentVersion'", error.ErrorMessage);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void VersionRange_may_be_empty(string value)
        {
            // ARRANGE
            using var repositoryDirectory = new TemporaryDirectory();
            var parameters = new CommandLineParameters()
            {
                RepositoryPath = repositoryDirectory,
                VersionRange = value
            };

            var validator = new CommandLineParametersValidator();

            // ACT 
            var result = validator.Validate(parameters);

            // ASSERT
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Theory]
        [InlineData("not-a-version-range")]
        public void VersionRange_must_be_a_valid_version_range_if_parameter_is_set(string version)
        {
            // ARRANGE
            using var repositoryDirectory = new TemporaryDirectory();

            var parameters = new CommandLineParameters()
            {
                RepositoryPath = repositoryDirectory,
                VersionRange = version
            };

            var validator = new CommandLineParametersValidator();

            // ACT 
            var result = validator.Validate(parameters);

            // ASSERT
            Assert.False(result.IsValid);
            var error = Assert.Single(result.Errors);
            Assert.Equal(nameof(CommandLineParameters.VersionRange), error.PropertyName);

            // error message should contain commandline parameter name (specified using the Option attribute) instead of the property name
            Assert.Contains("'versionRange'", error.ErrorMessage);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void GitHubAccessToken_may_be_empty(string value)
        {
            // ARRANGE
            using var repositoryDirectory = new TemporaryDirectory();
            var parameters = new CommandLineParameters()
            {
                RepositoryPath = repositoryDirectory,
                GitHubAccessToken = value
            };

            var validator = new CommandLineParametersValidator();

            // ACT 
            var result = validator.Validate(parameters);

            // ASSERT
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void GitLabAccessToken_may_be_empty(string value)
        {
            // ARRANGE
            using var repositoryDirectory = new TemporaryDirectory();
            var parameters = new CommandLineParameters()
            {
                RepositoryPath = repositoryDirectory,
                GitLabAccessToken = value
            };

            var validator = new CommandLineParametersValidator();

            // ACT 
            var result = validator.Validate(parameters);

            // ASSERT
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void ConfigurationFilePath_may_be_empty(string path)
        {
            // ARRANGE
            using var repositoryDirectory = new TemporaryDirectory();
            var parameters = new CommandLineParameters()
            {
                RepositoryPath = repositoryDirectory,
                ConfigurationFilePath = path
            };

            var validator = new CommandLineParametersValidator();

            // ACT 
            var result = validator.Validate(parameters);

            // ASSERT
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void ConfigurationFilePath_must_exists_if_parameter_is_set()
        {
            // ARRANGE
            using var repositoryDirectory = new TemporaryDirectory();
            var configurationFilePath = Path.Combine(repositoryDirectory, "config.json");

            var parameters = new CommandLineParameters()
            {
                RepositoryPath = repositoryDirectory,
                ConfigurationFilePath = configurationFilePath
            };

            var validator = new CommandLineParametersValidator();

            // ACT 
            var result = validator.Validate(parameters);

            // ASSERT
            Assert.False(result.IsValid);
            var error = Assert.Single(result.Errors);
            Assert.Equal(nameof(CommandLineParameters.ConfigurationFilePath), error.PropertyName);

            // error message should contain commandline parameter name (specified using the Option attribute) instead of the property name
            Assert.Contains("'configurationFilePath'", error.ErrorMessage);
        }
    }
}
