using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Buffered;
using Xunit.Abstractions;

namespace Grynwald.ChangeLog.Test
{
    /// <summary>
    /// Extension methods for <see cref="CliWrap.Command"/>
    /// </summary>
    public static class CommandExtensions
    {
        public static Command AddTestOutputPipe(this Command command, ITestOutputHelper testOutputHelper)
        {
            return command
                .WithStandardOutputPipe(
                     PipeTarget.Merge(
                        command.StandardOutputPipe,
                        PipeTarget.ToDelegate(testOutputHelper.WriteLine)))
                .WithStandardErrorPipe(
                    PipeTarget.Merge(
                    command.StandardErrorPipe,
                    PipeTarget.ToDelegate(testOutputHelper.WriteLine)));
        }

        public static async Task<BufferedCommandResult> ExecuteBufferedWithTestOutputAsync(this Command command, ITestOutputHelper testOutputHelper, string? commandId = null)
        {
            command = command.AddTestOutputPipe(testOutputHelper);

            testOutputHelper.WriteLine("------------------------------------------------------------");
            testOutputHelper.WriteLine($"BEGIN Command '{command.TargetFilePath} {command.Arguments}'");
            testOutputHelper.WriteLine($"Working directory: '{command.WorkingDirPath}'");
            if (commandId is not null)
            {
                testOutputHelper.WriteLine($"Command Id: '{commandId}'");
            }

            var changelogEnvironmentVariables = Environment.GetEnvironmentVariables().Cast<DictionaryEntry>()
                .Select(x => new KeyValuePair<string, string?>((string)x.Key, (string?)x.Value))
                .Where(x => x.Key.StartsWith("CHANGELOG_", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            testOutputHelper.WriteLine("CHANGELOG_ environment variables:");
            if (changelogEnvironmentVariables.Any())
            {
                foreach (var (name, value) in changelogEnvironmentVariables)
                {
                    testOutputHelper.WriteLine($"    {name}: '{value ?? ""}'");
                }
            }
            else
            {
                testOutputHelper.WriteLine("    None");
            }
            testOutputHelper.WriteLine("------------------------------------------------------------");

            var result = await command.ExecuteBufferedAsync();

            testOutputHelper.WriteLine("------------------------------------------------------------");
            testOutputHelper.WriteLine($"END Command, exit code: {result.ExitCode}");
            testOutputHelper.WriteLine("------------------------------------------------------------");
            testOutputHelper.WriteLine("");

            return result;
        }
    }
}
