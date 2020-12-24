using System;
using System.Diagnostics;
using System.Text;
using Grynwald.ChangeLog.Git;
using Xunit.Abstractions;

namespace Grynwald.ChangeLog.Test.Git
{
    /// <summary>
    /// Provides an wrapper around the git CLI for testing
    /// </summary>
    internal class GitWrapper
    {
        private readonly string m_WorkingDirectory;
        private readonly ITestOutputHelper m_Output;

        /// <summary>
        /// Initializes a new instance of <see cref="GitWrapper"/>
        /// </summary>
        /// <param name="workingDirectory">The working directory to start git commands in</param>
        /// <param name="output">The output helper to write git's output to.</param>
        public GitWrapper(string workingDirectory, ITestOutputHelper output)
        {
            if (String.IsNullOrWhiteSpace(workingDirectory))
                throw new ArgumentException("Value must not be null or whitespace", nameof(workingDirectory));

            m_WorkingDirectory = workingDirectory;
            m_Output = output ?? throw new ArgumentNullException(nameof(output));
        }


        /// <summary>
        /// Initializes a new git repository in the working directory
        /// </summary>
        public void Init() => Exec("init");

        /// <summary>
        /// Creates a new commit
        /// </summary>
        public GitCommit Commit(string commitMessage)
        {
            Exec($"commit --allow-empty -m \"{commitMessage}\"");
            Exec("config --local user.name", out var userName);
            Exec("config --local user.email", out var userEmail);
            Exec("rev-parse HEAD", out var commitId);
            Exec("rev-parse --short HEAD", out var abbreviatedCommitId);
            Exec("log -1 --pretty=\"format:%cI\" ", out var date);

            return new GitCommit(
                new GitId(commitId.Trim(), abbreviatedCommitId.Trim()),
                $"{commitMessage}\n",
                DateTime.Parse(date.Trim()),
                new GitAuthor(userName.Trim(), userEmail.Trim())
            );
        }

        /// <summary>
        /// Creates a new git tag
        /// </summary>
        public GitTag Tag(string name, GitCommit commit)
        {
            Exec($"tag {name} {commit.Id}");
            return new GitTag(name, commit.Id);
        }

        /// <summary>
        /// Sets the specified git setting to the specified value.
        /// </summary>
        public void Config(string name, string value) => Exec($"config --local \"{name}\" \"{value}\"");

        /// <summary>
        /// Adds the specified remote to the repository
        /// </summary>
        public void AddRemote(string name, string url) => Exec($"remote add \"{name}\" \"{url}\"");

        /// <summary>
        /// Perfoms a git checkout operation
        /// </summary>
        /// <param name="ref">The git reference to check out</param>
        public void Checkout(string @ref) => Exec($"checkout \"{@ref}\"");

        /// <summary>
        /// Checks out a new branch created from the current HEAD commit
        /// </summary
        public void CheckoutNewBranch(string branchName) => Exec($"checkout -b \"{branchName}\"");


        private void Exec(string command) =>
            Exec(command, out _, out _);

        private void Exec(string command, out string stdOut) =>
            Exec(command, out stdOut, out _);

        private void Exec(string command, out string stdOut, out string stdErr)
        {
            var startInfo = new ProcessStartInfo()
            {
                FileName = "git",
                Arguments = command,
                WorkingDirectory = m_WorkingDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            var stdOutBuilder = new StringBuilder();
            var stdErrBuilder = new StringBuilder();

            var process = Process.Start(startInfo);

            if (process is null)
                throw new InvalidOperationException("Failed to start git process. Process.Start() returned null");

            process.ErrorDataReceived += (s, e) =>
            {
                if (e.Data is string)
                    stdErrBuilder.AppendLine(e.Data);
            };

            process.OutputDataReceived += (s, e) =>
            {
                if (e.Data is string)
                    stdOutBuilder.AppendLine(e.Data);
            };

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.WaitForExit();

            process.CancelErrorRead();
            process.CancelOutputRead();

            stdOut = stdOutBuilder.ToString();
            stdErr = stdErrBuilder.ToString();

            m_Output.WriteLine("--------------------------------");
            m_Output.WriteLine($"Begin Command 'git {command}'");
            m_Output.WriteLine("--------------------------------");
            m_Output.WriteLine("StdOut:");
            m_Output.WriteLine(stdOut);
            m_Output.WriteLine("StdErr:");
            m_Output.WriteLine(stdErr);
            m_Output.WriteLine("--------------------------------");
            m_Output.WriteLine($"End Command 'git {command}'");
            m_Output.WriteLine("--------------------------------");


            if (process.ExitCode != 0)
            {
                throw new Exception($"Command 'git {command}' completed with exit code {process.ExitCode}");
            }

        }
    }
}
