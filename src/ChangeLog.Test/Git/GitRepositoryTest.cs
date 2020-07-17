using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Grynwald.ChangeLog.Git;
using Grynwald.Utilities.IO;
using Xunit;
using Xunit.Abstractions;

namespace Grynwald.ChangeLog.Test.Git
{
    /// <summary>
    /// Tests for <see cref="GitRepository"/>
    /// </summary>
    public class GitRepositoryTest : IDisposable
    {
        private readonly TemporaryDirectory m_WorkingDirectory = new TemporaryDirectory();
        private readonly ITestOutputHelper m_TestOutputHelper;

        public GitRepositoryTest(ITestOutputHelper testOutputHelper)
        {
            m_TestOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));

            Git("init");
            Git("config --local user.name Example");
            Git("config --local user.email user@example.com");
        }

        public void Dispose() => m_WorkingDirectory.Dispose();

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("\t")]
        public void Repository_path_must_not_be_null_or_whitespace(string repositoryPath)
        {
            Assert.Throws<ArgumentException>(() => new GitRepository(repositoryPath));
        }


        [Fact]
        public void Remotes_returns_expected_remotes_01()
        {
            // ARRANGE            
            using var sut = new GitRepository(m_WorkingDirectory);

            // ACT 
            var remotes = sut.Remotes.ToArray();

            // ASSERT
            Assert.NotNull(remotes);
            Assert.Empty(remotes);
        }

        [Fact]
        public void Remotes_returns_expected_remotes_02()
        {
            // ARRANGE
            Git("remote add origin https://example.com/origin");
            Git("remote add upstream https://example.com/upstream");

            using var sut = new GitRepository(m_WorkingDirectory);

            // ACT 
            var remotes = sut.Remotes;

            // ASSERT
            Assert.NotNull(remotes);
            Assert.Equal(2, remotes.Count());
            Assert.Contains(remotes, remote => remote.Name == "origin" && remote.Url == "https://example.com/origin");
            Assert.Contains(remotes, remote => remote.Name == "upstream" && remote.Url == "https://example.com/upstream");
        }

        [Fact]
        public void Head_returns_expected_commit()
        {
            // ARRANGE            
            var expectedHead = GitCommit("Initialize repository");

            using var sut = new GitRepository(m_WorkingDirectory);

            // ACT 
            var head = sut.Head;

            // ASSERT
            Assert.NotNull(head);
            Assert.Equal(expectedHead, head);
        }

        [Fact]
        public void GetTags_returns_expected_tags_01()
        {
            // ARRANGE
            using var sut = new GitRepository(m_WorkingDirectory);

            // ACT
            var tags = sut.GetTags();

            // ASSERT
            Assert.NotNull(tags);
            Assert.Empty(tags);
        }

        [Fact]
        public void GetTags_returns_expected_tags_02()
        {
            // ARRANGE
            var commit1 = GitCommit("First commit");
            var commit2 = GitCommit("Second commit");
            var commit3 = GitCommit("Third commit");

            var tag1 = GitTag("tag1", commit1);
            var tag2 = GitTag("tag2", commit2);
            var tag3 = GitTag("tag3", commit3);

            using var sut = new GitRepository(m_WorkingDirectory);

            // ACT
            var tags = sut.GetTags();

            // ASSERT
            Assert.NotNull(tags);
            Assert.Contains(tags, t => t.Equals(tag1));
            Assert.Contains(tags, t => t.Equals(tag2));
            Assert.Contains(tags, t => t.Equals(tag3));
        }


        [Fact]
        public void GetCommits_returns_the_expected_commits_01()
        {
            // ARRANGE
            var expectedCommit = GitCommit("commit 1");
            using var sut = new GitRepository(m_WorkingDirectory);

            // ACT 
            var commits = sut.GetCommits(null, expectedCommit.Id);

            // ASSERT
            Assert.NotNull(commits);
            var commit = Assert.Single(commits);
            Assert.Equal(expectedCommit, commit);
        }

        [Fact]
        public void GetCommits_returns_the_expected_commits_02()
        {
            // ARRANGE
            var expectedCommit1 = GitCommit("commit 1");
            var expectedCommit2 = GitCommit("commit 2");
            var expectedCommit3 = GitCommit("commit 3");

            using var sut = new GitRepository(m_WorkingDirectory);

            // ACT 
            var commits = sut.GetCommits(null, expectedCommit3.Id);

            // ASSERT
            Assert.NotNull(commits);
            Assert.Equal(3, commits.Count);
            Assert.Contains(commits, c => c.Equals(expectedCommit1));
            Assert.Contains(commits, c => c.Equals(expectedCommit2));
            Assert.Contains(commits, c => c.Equals(expectedCommit3));
        }

        [Fact]
        public void GetCommits_returns_the_expected_commits_03()
        {
            // ARRANGE
            var expectedCommit1 = GitCommit("commit 1");
            Git("checkout -b branch1");
            var expectedCommit2 = GitCommit("commit 2");
            var expectedCommit3 = GitCommit("commit 3");
            Git("checkout -");
            var expectedCommit4 = GitCommit("commit 4");
            var expectedCommit5 = GitCommit("commit 5");

            using var sut = new GitRepository(m_WorkingDirectory);

            // ACT 
            var commits = sut.GetCommits(null, expectedCommit5.Id);

            // ASSERT
            Assert.NotNull(commits);
            Assert.Equal(3, commits.Count);
            Assert.Contains(commits, c => c.Equals(expectedCommit1));
            Assert.Contains(commits, c => c.Equals(expectedCommit4));
            Assert.Contains(commits, c => c.Equals(expectedCommit5));
        }

        [Fact]
        public void GetCommits_returns_the_expected_commits_04()
        {
            // ARRANGE
            var expectedCommit1 = GitCommit("commit 1");
            var expectedCommit2 = GitCommit("commit 2");
            var expectedCommit3 = GitCommit("commit 3");
            var expectedCommit4 = GitCommit("commit 4");

            using var sut = new GitRepository(m_WorkingDirectory);

            // ACT 
            var commits = sut.GetCommits(expectedCommit2.Id, expectedCommit4.Id);

            // ASSERT
            Assert.NotNull(commits);
            Assert.Equal(2, commits.Count);
            Assert.Contains(commits, c => c.Equals(expectedCommit3));
            Assert.Contains(commits, c => c.Equals(expectedCommit4));
        }

        [Fact]
        public void GetCommits_returns_the_expected_commits_05()
        {
            // ARRANGE
            var expectedCommit1 = GitCommit("commit 1");
            Git("checkout -b branch1");
            var expectedCommit2 = GitCommit("commit 2");
            var expectedCommit3 = GitCommit("commit 3");
            Git("checkout -");
            var expectedCommit4 = GitCommit("commit 4");
            var expectedCommit5 = GitCommit("commit 5");

            using var sut = new GitRepository(m_WorkingDirectory);

            // ACT 
            var commits = sut.GetCommits(expectedCommit3.Id, expectedCommit5.Id);

            // ASSERT
            Assert.NotNull(commits);
            Assert.Equal(2, commits.Count);
            Assert.Contains(commits, c => c.Equals(expectedCommit4));
            Assert.Contains(commits, c => c.Equals(expectedCommit5));
        }

        private GitTag GitTag(string name, GitCommit commit)
        {
            Git($"tag {name} {commit.Id}");
            return new GitTag(name, commit.Id);
        }

        private GitCommit GitCommit(string commitMessage)
        {
            Git($"commit --allow-empty -m \"{commitMessage}\"");
            Git("config --local user.name", out var userName);
            Git("config --local user.email", out var userEmail);
            Git("rev-parse --short HEAD", out var commitId);
            Git("log -1 --pretty=\"format:%cI\" ", out var date);

            return new GitCommit(
                new GitId(commitId.Trim()),
                $"{commitMessage}\n",
                DateTime.Parse(date.Trim()),
                new GitAuthor(userName.Trim(), userEmail.Trim())
            );
        }

        private void Git(string command) =>
            Git(command, out _, out _);

        private void Git(string command, out string stdOut) =>
            Git(command, out stdOut, out _);

        private void Git(string command, out string stdOut, out string stdErr)
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

            m_TestOutputHelper.WriteLine("--------------------------------");
            m_TestOutputHelper.WriteLine($"Begin Command 'git {command}'");
            m_TestOutputHelper.WriteLine("--------------------------------");
            m_TestOutputHelper.WriteLine("StdOut:");
            m_TestOutputHelper.WriteLine(stdOut);
            m_TestOutputHelper.WriteLine("StdErr:");
            m_TestOutputHelper.WriteLine(stdErr);
            m_TestOutputHelper.WriteLine("--------------------------------");
            m_TestOutputHelper.WriteLine($"End Command 'git {command}'");
            m_TestOutputHelper.WriteLine("--------------------------------");


            if (process.ExitCode != 0)
            {
                throw new Exception($"Command 'git {command}' completed with exit code {process.ExitCode}");
            }

        }
    }
}
