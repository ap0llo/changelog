using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Buffered;
using Grynwald.ChangeLog.Test.Git;
using Grynwald.Utilities.IO;
using Xunit;
using Xunit.Abstractions;

namespace Grynwald.ChangeLog.Test.E2E
{
    /// <summary>
    /// End-to-End tests that run "changelog" as a separate process on a real (created for the test) git repository
    /// </summary>
    // This test class launches changelog as a child process which inherits the test process' environment variables
    // To avoid the test from inheriting environment variables it shouldn't, add it to the test "EnvironmentVariable"
    // test collections to prevent the tests from running in parallel with test that modify environment variables
    [Collection(nameof(EnvironmentVariableCollection))]
    public class E2ETests
    {
        private readonly ITestOutputHelper m_TestOutputHelper;

        public E2ETests(ITestOutputHelper testOutputHelper)
        {
            m_TestOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
        }


        [Theory]
        [InlineData("--help")]
        [InlineData("help")]
        public async Task Requesting_help_succeeds(string helpCommand)
        {
            // ARRANGE
            var applicationVersion = typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

            // ACT 
            var result = await RunApplicationAsync(
                args: new[] { helpCommand },
                commandId: nameof(Requesting_help_succeeds)
            );

            // ASSERT
            Assert.Equal(0, result.ExitCode);
            Assert.Contains($"changelog {applicationVersion}{Environment.NewLine}", result.StandardOutput);
            Assert.Contains("generate, g", result.StandardOutput);
            Assert.Contains("help", result.StandardOutput);
            Assert.Contains("version", result.StandardOutput);
            Assert.Empty(result.StandardError);
        }

        [Theory]
        [InlineData(new object[] { new string[] { "help", "generate" } })]
        [InlineData(new object[] { new string[] { "generate", "--help" } })]
        public async Task Requesting_help_for_the_generate_command_succeeds(string[] helpCommand)
        {
            // ARRANGE
            var applicationVersion = typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

            // ACT 
            var result = await RunApplicationAsync(
                args: helpCommand,
                commandId: nameof(Requesting_help_succeeds)
            );

            // ASSERT
            Assert.Equal(0, result.ExitCode);
            Assert.Contains($"changelog {applicationVersion}{Environment.NewLine}", result.StandardOutput);
            Assert.Contains("-r, --repository", result.StandardOutput);
            Assert.Contains("-c, --configurationFilePath", result.StandardOutput);
            Assert.Contains("-o, --outputPath", result.StandardOutput);
            Assert.Contains("--githubAccessToken", result.StandardOutput);
            Assert.Contains("--gitlabAccessToken", result.StandardOutput);
            Assert.Contains("--versionRange", result.StandardOutput);
            Assert.Contains("--currentVersion", result.StandardOutput);
            Assert.Contains("--template", result.StandardOutput);
            Assert.Contains("--help", result.StandardOutput);
            Assert.Contains("--version", result.StandardOutput);
            Assert.Empty(result.StandardError);
        }


        [Theory]
        [InlineData("version")]
        [InlineData("--version")]
        public async Task Requesting_version_succeeds(string versionCommand)
        {
            // ARRANGE
            var expectedVersion = typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

            // ACT 
            var result = await RunApplicationAsync(
                args: new[] { versionCommand },
                commandId: nameof(Requesting_version_succeeds)
            );

            // ASSERT
            Assert.Equal(0, result.ExitCode);
            Assert.Equal($"changelog {expectedVersion}", result.StandardOutput.Trim());
            Assert.Empty(result.StandardError);
        }

        [Fact]
        public async Task Change_log_is_generated_from_the_specified_repository()
        {
            // ARRANGE
            using var temporaryDirectory = new TemporaryDirectory();

            var git = new GitWrapper(temporaryDirectory, m_TestOutputHelper);
            await git.InitAsync();
            await git.ConfigAsync("user.name", "Example");
            await git.ConfigAsync("user.email", "user@example.com");

            var commit = await git.CommitAsync("feat: Some New feature");
            await git.TagAsync("v1.0.0", commit);

            await Task.Delay(500);

            var expectedOutputPath = Path.Combine(temporaryDirectory, "changelog.md");
            var expectedOutput = String.Join(Environment.NewLine,
                "# Change Log",
                "",
                "## 1.0.0",
                "",
                $"#### <a id=\"changelog-heading-{commit.Id.Id}\"></a> Some New feature",
                "",
                $"- Commit: `{commit.Id.AbbreviatedId}`",
                "");

            // ACT 
            var result = await RunApplicationAsync(
                args: new[] { "--repository", temporaryDirectory },
                commandId: nameof(Change_log_is_generated_from_the_specified_repository)
            );

            // ASSERT
            Assert.Equal(0, result.ExitCode);
            Assert.True(File.Exists(expectedOutputPath));
            Assert.Equal(expectedOutput, File.ReadAllText(expectedOutputPath));
        }

        [Fact]
        public async Task Repository_path_can_be_passed_as_relative_path()
        {
            // ARRANGE
            using var temporaryDirectory = new TemporaryDirectory();
            var repositoryPath = temporaryDirectory.AddSubDirectory("repo");

            var git = new GitWrapper(repositoryPath, m_TestOutputHelper);
            await git.InitAsync();
            await git.ConfigAsync("user.name", "Example");
            await git.ConfigAsync("user.email", "user@example.com");

            var commit = await git.CommitAsync("feat: Some New feature");
            await git.TagAsync("v1.0.0", commit);

            await Task.Delay(500);

            var expectedOutputPath = Path.Combine(repositoryPath, "changelog.md");

            // ACT 
            var result = await RunApplicationAsync(
                args: new[] { "--repository", "repo" },
                workingDirectory: temporaryDirectory,
                commandId: nameof(Repository_path_can_be_passed_as_relative_path)
            );

            // ASSERT
            Assert.Equal(0, result.ExitCode);
            Assert.True(File.Exists(expectedOutputPath));
        }

        [Theory]
        [InlineData("")]
        [InlineData("dir1")]
        [InlineData("dir1/dir2")]
        public async Task When_no_repository_is_specified_the_repository_is_located_from_the_current_directory(string relativeWorkingDirectoryPath)
        {
            // ARRANGE
            using var temporaryDirectory = new TemporaryDirectory();
            var workingDirectory = temporaryDirectory.AddSubDirectory(relativeWorkingDirectoryPath);

            var git = new GitWrapper(temporaryDirectory, m_TestOutputHelper);
            await git.InitAsync();
            await git.ConfigAsync("user.name", "Example");
            await git.ConfigAsync("user.email", "user@example.com");

            var commit = await git.CommitAsync("feat: Some New feature");
            await git.TagAsync("v1.0.0", commit);

            await Task.Delay(500);

            var expectedOutputPath = Path.Combine(temporaryDirectory, "changelog.md");
            var expectedOutput = String.Join(Environment.NewLine,
                "# Change Log",
                "",
                "## 1.0.0",
                "",
                $"#### <a id=\"changelog-heading-{commit.Id.Id}\"></a> Some New feature",
                "",
                $"- Commit: `{commit.Id.AbbreviatedId}`",
                "");

            // ACT 
            var result = await RunApplicationAsync(
                args: new[] { "--verbose" },
                workingDirectory: workingDirectory,
                commandId: $"{nameof(When_no_repository_is_specified_the_repository_is_located_from_the_current_directory)}(\"{relativeWorkingDirectoryPath}\")"
            );

            // ASSERT
            Assert.Equal(0, result.ExitCode);
            Assert.True(File.Exists(expectedOutputPath));
            Assert.Equal(expectedOutput, File.ReadAllText(expectedOutputPath));
        }

        [Fact]
        public async Task When_specified_repository_path_is_not_a_git_repository_an_error_is_shown()
        {
            // ARRANGE
            using var temporaryDirectory = new TemporaryDirectory();

            // ACT 
            var result = await RunApplicationAsync(
                args: new[] { "--repository", temporaryDirectory },
                commandId: nameof(When_specified_repository_path_is_not_a_git_repository_an_error_is_shown)
            );

            // ASSERT
            Assert.Equal(1, result.ExitCode);
            Assert.Contains(temporaryDirectory.FullName, result.StandardOutput);
            Assert.Contains("is not a git repository", result.StandardOutput, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task When_started_outside_of_a_git_repository_and_no_repository_path_is_specified_an_error_is_shown()
        {
            // ARRANGE
            using var temporaryDirectory = new TemporaryDirectory();

            // ACT 
            var result = await RunApplicationAsync(
                args: Array.Empty<string>(),
                workingDirectory: temporaryDirectory,
                commandId: nameof(When_started_outside_of_a_git_repository_and_no_repository_path_is_specified_an_error_is_shown)
            );

            // ASSERT
            Assert.Equal(1, result.ExitCode);
            Assert.Contains(temporaryDirectory.FullName, result.StandardOutput);
            Assert.Contains("no git repository found", result.StandardOutput, StringComparison.OrdinalIgnoreCase);
        }


        [Theory]
        // When no confguration file exists on disk and the configurationFilePath commandline parameter is not set, use default settings
        [InlineData(new string[0], null, null)]
        // When a configuration file exists at one of the default locations, this file is used
        [InlineData(new string[] { "changelog.settings.json" }, null, "changelog.settings.json")]
        [InlineData(new string[] { ".config/changelog/settings.json" }, null, ".config/changelog/settings.json")]
        // When a configuration file exists at a default location and an additional config file exists, the file at the default location is used
        [InlineData(new string[] { "changelog.settings.json", "custom.settings.json" }, null, "changelog.settings.json")]
        [InlineData(new string[] { ".config/changelog/settings.json", "custom.settings.json" }, null, ".config/changelog/settings.json")]
        // When files at multiple default locations exists, "changelog.settings.json" takes precedence (for backwards compatibility)
        [InlineData(new string[] { ".config/changelog/settings.json", "changelog.settings.json" }, null, "changelog.settings.json")]
        // When the configurationFilePath commandline parameter is set, the value from commandline is used
        [InlineData(new string[] { "changelog.settings.json", "custom.settings.json" }, "custom.settings.json", "custom.settings.json")]
        [InlineData(new string[] { ".config/changelog/settings.json", "custom.settings.json" }, "custom.settings.json", "custom.settings.json")]
        [InlineData(new string[] { "changelog.settings.json", ".config/changelog/settings.json", "custom.settings.json" }, "custom.settings.json", "custom.settings.json")]
        public async Task The_expected_configuration_file_is_used(string[] configurationFilesOnDisk, string? configurationFileParameter, string? expectedConfigurationFile)
        {
            // ARRANGE
            using var temporaryDirectory = new TemporaryDirectory();

            var git = new GitWrapper(temporaryDirectory, m_TestOutputHelper);
            await git.InitAsync();
            await git.ConfigAsync("user.name", "Example");
            await git.ConfigAsync("user.email", "user@example.com");

            // Create all configuration files with distinct output path settings
            var outputNames = new Dictionary<string, string>();
            foreach (var configurationFilePath in configurationFilesOnDisk)
            {
                var outputName = $"{Guid.NewGuid()}.md";
                temporaryDirectory.AddFile(
                    configurationFilePath,
                    $@"{{ ""changelog"" : {{ ""outputPath"" : ""{outputName}"" }} }}"
                );

                outputNames.Add(configurationFilePath, outputName);
            }

            // Determine the expected output path (based on the output path we can determine which configuration file was used).
            // If none of the configuration file is expected to be used (expectedConfigurationFile is null), expect the changelog to be written to the default location
            var expectedOutputPath = expectedConfigurationFile is null
                ? Path.Combine(temporaryDirectory, "changelog.md")
                : Path.Combine(temporaryDirectory, outputNames[expectedConfigurationFile]);

            var args = new List<string>() { "--verbose" };
            // When specified, append the configurationFilePath commandline parameter
            if (configurationFileParameter is not null)
            {
                args.Add("--configurationFilePath");
                args.Add(configurationFileParameter);
            }

            // ACT
            var result = await RunApplicationAsync(
                args: args,
                workingDirectory: temporaryDirectory,
                commandId: $"{nameof(The_expected_configuration_file_is_used)}([{String.Join(",", configurationFilesOnDisk)}], \"{configurationFileParameter}\", \"{expectedConfigurationFile}\")"
            );

            // ASSERT
            Assert.Equal(0, result.ExitCode);
            Assert.True(File.Exists(expectedOutputPath));
        }


        /// <summary>
        /// Runs changelog with the specified command line parameters
        /// </summary>
        private async Task<BufferedCommandResult> RunApplicationAsync(IReadOnlyList<string> args, string? workingDirectory = null, string? commandId = null)
        {
            var applicationAssembly = typeof(Program).Assembly;

            // The application is published to the test output directory into the "Grynwald.ChangeLog" directory
            var assemblyPath = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
                "Grynwald.ChangeLog",
                Path.GetFileName(applicationAssembly.Location));

            // 
            // Note: It is important to use the changelog.dll from the test directory rather
            // than the copy from the applications build output directory.
            // Coverlet instruments all assemblies in the test directory in order to compute the tests' code coverage.
            // When running the instrumented assembly (even in a child process),
            // the covered lines will be included in the computed coverage.
            //
            // If the test ran the changelog.dll copied to the publish directory
            // the test would still work but would not be included in the computed code coverage.
            //
            // To resolve that, replace the changelog assembly in the publish directory with the instrumented assembly
            File.Copy(applicationAssembly.Location, assemblyPath, true);

            var command = Cli.Wrap("dotnet")
                .WithArguments(dotnetArgs => dotnetArgs
                    .Add(assemblyPath)
                    .Add(args))
                .WithValidation(CommandResultValidation.None);

            if (workingDirectory is not null)
            {
                command = command.WithWorkingDirectory(workingDirectory);
            }

            var result = await command.ExecuteBufferedWithTestOutputAsync(m_TestOutputHelper, commandId: commandId);

            return result;
        }
    }
}
