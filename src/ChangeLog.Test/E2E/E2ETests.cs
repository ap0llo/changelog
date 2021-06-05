using System;
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
    public class E2ETests
    {
        private readonly ITestOutputHelper m_TestOutputHelper;

        public E2ETests(ITestOutputHelper testOutputHelper)
        {
            m_TestOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
        }


        [Fact]
        public async Task Requesting_help_succeeds()
        {
            // ARRANGE
            var applicationVersion = typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

            // ACT 
            var result = await RunApplicationAsync(
                args: new[] { "--help" },
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

        [Fact]
        public async Task Requesting_version_succeeds()
        {
            // ARRANGE
            var expectedVersion = typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

            // ACT 
            var result = await RunApplicationAsync(
                args: new[] { "--version" },
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


        /// <summary>
        /// Runs changelog with the specified command line parameters
        /// </summary>
        private async Task<BufferedCommandResult> RunApplicationAsync(string[] args, string? workingDirectory = null, string? commandId = null)
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
