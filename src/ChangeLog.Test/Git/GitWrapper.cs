using System;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Buffered;
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
        public Task InitAsync() => ExecAsync("init");

        /// <summary>
        /// Creates a new commit
        /// </summary>
        public async Task<GitCommit> CommitAsync(string commitMessage)
        {
            await ExecAsync($"commit --allow-empty -m \"{commitMessage}\"");
            var userName = (await ExecAsync("config --local user.name")).StandardOutput;
            var userEmail = (await ExecAsync("config --local user.email")).StandardOutput;

            var commitId = (await ExecAsync("rev-parse HEAD")).StandardOutput;
            var abbreviatedCommitId = (await ExecAsync("rev-parse --short HEAD")).StandardOutput;
            var date = (await ExecAsync("log -1 --pretty=\"format:%cI\" ")).StandardOutput;

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
        public async Task<GitTag> TagAsync(string name, GitCommit commit)
        {
            await ExecAsync($"tag {name} {commit.Id}");
            return new GitTag(name, commit.Id);
        }

        /// <summary>
        /// Sets the specified git setting to the specified value.
        /// </summary>
        public Task ConfigAsync(string name, string value) => ExecAsync($"config --local \"{name}\" \"{value}\"");

        /// <summary>
        /// Adds the specified remote to the repository
        /// </summary>
        public Task AddRemoteAsync(string name, string url) => ExecAsync($"remote add \"{name}\" \"{url}\"");

        /// <summary>
        /// Perfoms a git checkout operation
        /// </summary>
        /// <param name="ref">The git reference to check out</param>
        public Task CheckoutAsync(string @ref) => ExecAsync($"checkout \"{@ref}\"");

        /// <summary>
        /// Checks out a new branch created from the current HEAD commit
        /// </summary
        public Task CheckoutNewBranchAsync(string branchName) => ExecAsync($"checkout -b \"{branchName}\"");


        private Task<BufferedCommandResult> ExecAsync(string command)
        {
            return Cli.Wrap("git")
                .WithValidation(CommandResultValidation.ZeroExitCode)
                .WithArguments(command)
                .WithWorkingDirectory(m_WorkingDirectory)
                .ExecuteBufferedWithTestOutputAsync(m_Output);
        }
    }
}
