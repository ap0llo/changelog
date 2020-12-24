using System;
using System.Linq;
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
        private readonly TemporaryDirectory m_WorkingDirectory;

        private GitWrapper Git { get; }

        public GitRepositoryTest(ITestOutputHelper testOutputHelper)
        {
            if (testOutputHelper is null)
                throw new ArgumentNullException(nameof(testOutputHelper));

            m_WorkingDirectory = new TemporaryDirectory();
            Git = new GitWrapper(m_WorkingDirectory, testOutputHelper);

            Git.Init();
            Git.Config("user.name", "Example");
            Git.Config("user.email", "user@example.com");
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
            Git.AddRemote("origin", "https://example.com/origin");
            Git.AddRemote("upstream", "https://example.com/upstream");

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
            var expectedHead = Git.Commit("Initialize repository");

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
            var commit1 = Git.Commit("First commit");
            var commit2 = Git.Commit("Second commit");
            var commit3 = Git.Commit("Third commit");

            var tag1 = Git.Tag("tag1", commit1);
            var tag2 = Git.Tag("tag2", commit2);
            var tag3 = Git.Tag("tag3", commit3);

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
            var expectedCommit = Git.Commit("commit 1");
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
            var expectedCommit1 = Git.Commit("commit 1");
            var expectedCommit2 = Git.Commit("commit 2");
            var expectedCommit3 = Git.Commit("commit 3");

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
            var expectedCommit1 = Git.Commit("commit 1");
            Git.CheckoutNewBranch("branch1");
            var expectedCommit2 = Git.Commit("commit 2");
            var expectedCommit3 = Git.Commit("commit 3");
            Git.Checkout("-");
            var expectedCommit4 = Git.Commit("commit 4");
            var expectedCommit5 = Git.Commit("commit 5");

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
            var expectedCommit1 = Git.Commit("commit 1");
            var expectedCommit2 = Git.Commit("commit 2");
            var expectedCommit3 = Git.Commit("commit 3");
            var expectedCommit4 = Git.Commit("commit 4");

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
            var expectedCommit1 = Git.Commit("commit 1");
            Git.CheckoutNewBranch("branch1");
            var expectedCommit2 = Git.Commit("commit 2");
            var expectedCommit3 = Git.Commit("commit 3");
            Git.Checkout("-");
            var expectedCommit4 = Git.Commit("commit 4");
            var expectedCommit5 = Git.Commit("commit 5");

            using var sut = new GitRepository(m_WorkingDirectory);

            // ACT 
            var commits = sut.GetCommits(expectedCommit3.Id, expectedCommit5.Id);

            // ASSERT
            Assert.NotNull(commits);
            Assert.Equal(2, commits.Count);
            Assert.Contains(commits, c => c.Equals(expectedCommit4));
            Assert.Contains(commits, c => c.Equals(expectedCommit5));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        public void TryGetCommit_throws_ArgumentException_if_id_is_null_or_whitespace(string id)
        {
            // ARRANGE
            using var sut = new GitRepository(m_WorkingDirectory);

            // ACT 
            var ex = Record.Exception(() => sut.TryGetCommit(id));

            // ASSERT
            Assert.NotNull(ex);
            var argumentException = Assert.IsType<ArgumentException>(ex);
            Assert.Equal("id", argumentException.ParamName);
        }

        [Theory]
        [InlineData(7)]
        [InlineData(23)]
        [InlineData(40)]
        public void TryGetCommit_returns_the_expected_commit(int idLength)
        {
            // ARRANGE
            var expectedCommit = Git.Commit("Commit 1");
            _ = Git.Commit("Commit 2");

            using var sut = new GitRepository(m_WorkingDirectory);

            // ACT
            var actualCommit = sut.TryGetCommit(expectedCommit.Id.Id.Substring(0, idLength));

            // ASSERT
            Assert.NotNull(actualCommit);
            Assert.Equal(expectedCommit, actualCommit);
        }

        [Theory]
        [InlineData("not-a-commit-id")]
        [InlineData("abcd1234")]
        public void TryGetCommit_returns_null_if_commit_cannot_be_found(string id)
        {
            // ARRANGE
            _ = Git.Commit("Commit 1");
            _ = Git.Commit("Commit 2");

            using var sut = new GitRepository(m_WorkingDirectory);

            // ACT 
            var commit = sut.TryGetCommit(id);

            // ASSERT
            Assert.Null(commit);
        }

        [Fact]
        public void TryGetCommit_is_case_insensitive()
        {
            // ARRANGE
            var expectedCommit = Git.Commit("Commit 1");
            _ = Git.Commit("Commit 2");

            using var sut = new GitRepository(m_WorkingDirectory);

            // ACT
            var actualCommit = sut.TryGetCommit(expectedCommit.Id.Id.ToUpper());

            // ASSERT
            Assert.NotNull(actualCommit);
            Assert.Equal(expectedCommit, actualCommit);
        }



    }
}
