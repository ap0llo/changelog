using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Grynwald.Utilities.IO;
using Xunit;

namespace Grynwald.ChangeLog.Test
{
    public class CommandLineParametersValidatorTest
    {
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("  ")]
        [InlineData("\t")]
        public void RepositoryPath_must_not_be_null_or_empty(string repositoryPath)
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
    }
}
