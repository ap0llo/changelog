using System;
using System.Linq;
using System.Threading.Tasks;
using Grynwald.ChangeLog.Git;
using Grynwald.Utilities.IO;
using Xunit;
using Xunit.Abstractions;

namespace Grynwald.ChangeLog.Test.Git
{
    /// <summary>
    /// Tests for <see cref="GitRepository"/>
    /// </summary>
    public class GitRepositoryTest : IAsyncLifetime
    {
        private readonly TemporaryDirectory m_WorkingDirectory;

        private GitWrapper Git { get; }

        public GitRepositoryTest(ITestOutputHelper testOutputHelper)
        {
            if (testOutputHelper is null)
                throw new ArgumentNullException(nameof(testOutputHelper));

            m_WorkingDirectory = new TemporaryDirectory();
            Git = new GitWrapper(m_WorkingDirectory, testOutputHelper);
        }


        public async Task InitializeAsync()
        {
            await Git.InitAsync();
            await Git.ConfigAsync("user.name", "Example");
            await Git.ConfigAsync("user.email", "user@example.com");
        }

        public Task DisposeAsync()
        {
            m_WorkingDirectory.Dispose();
            return Task.CompletedTask;
        }


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
        public async Task Remotes_returns_expected_remotes_02()
        {
            // ARRANGE
            await Git.AddRemoteAsync("origin", "https://example.com/origin");
            await Git.AddRemoteAsync("upstream", "https://example.com/upstream");

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
        public async Task Head_returns_expected_commit()
        {
            // ARRANGE            
            var expectedHead = await Git.CommitAsync("Initialize repository");

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
        public async Task GetTags_returns_expected_tags_02()
        {
            // ARRANGE
            var commit1 = await Git.CommitAsync("First commit");
            var commit2 = await Git.CommitAsync("Second commit");
            var commit3 = await Git.CommitAsync("Third commit");

            var tag1 = await Git.TagAsync("tag1", commit1);
            var tag2 = await Git.TagAsync("tag2", commit2);
            var tag3 = await Git.TagAsync("tag3", commit3);

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
        public async Task GetCommits_returns_the_expected_commits_01()
        {
            // ARRANGE
            var expectedCommit = await Git.CommitAsync("commit 1");
            using var sut = new GitRepository(m_WorkingDirectory);

            // ACT 
            var commits = sut.GetCommits(null, expectedCommit.Id);

            // ASSERT
            Assert.NotNull(commits);
            var commit = Assert.Single(commits);
            Assert.Equal(expectedCommit, commit);
        }

        [Fact]
        public async Task GetCommits_returns_the_expected_commits_02()
        {
            // ARRANGE
            var expectedCommit1 = await Git.CommitAsync("commit 1");
            var expectedCommit2 = await Git.CommitAsync("commit 2");
            var expectedCommit3 = await Git.CommitAsync("commit 3");

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
        public async Task GetCommits_returns_the_expected_commits_03()
        {
            // ARRANGE
            var expectedCommit1 = await Git.CommitAsync("commit 1");
            await Git.CheckoutNewBranchAsync("branch1");
            var expectedCommit2 = await Git.CommitAsync("commit 2");
            var expectedCommit3 = await Git.CommitAsync("commit 3");
            await Git.CheckoutAsync("-");
            var expectedCommit4 = await Git.CommitAsync("commit 4");
            var expectedCommit5 = await Git.CommitAsync("commit 5");

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
        public async Task GetCommits_returns_the_expected_commits_04()
        {
            // ARRANGE
            var expectedCommit1 = await Git.CommitAsync("commit 1");
            var expectedCommit2 = await Git.CommitAsync("commit 2");
            var expectedCommit3 = await Git.CommitAsync("commit 3");
            var expectedCommit4 = await Git.CommitAsync("commit 4");

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
        public async Task GetCommits_returns_the_expected_commits_05()
        {
            // ARRANGE
            var expectedCommit1 = await Git.CommitAsync("commit 1");
            await Git.CheckoutNewBranchAsync("branch1");
            var expectedCommit2 = await Git.CommitAsync("commit 2");
            var expectedCommit3 = await Git.CommitAsync("commit 3");
            await Git.CheckoutAsync("-");
            var expectedCommit4 = await Git.CommitAsync("commit 4");
            var expectedCommit5 = await Git.CommitAsync("commit 5");

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
        public async Task TryGetCommit_returns_the_expected_commit(int idLength)
        {
            // ARRANGE
            var expectedCommit = await Git.CommitAsync("Commit 1");
            _ = await Git.CommitAsync("Commit 2");

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
        public async Task TryGetCommit_returns_null_if_commit_cannot_be_found(string id)
        {
            // ARRANGE
            _ = await Git.CommitAsync("Commit 1");
            _ = await Git.CommitAsync("Commit 2");

            using var sut = new GitRepository(m_WorkingDirectory);

            // ACT 
            var commit = sut.TryGetCommit(id);

            // ASSERT
            Assert.Null(commit);
        }

        [Fact]
        public async Task TryGetCommit_is_case_insensitive()
        {
            // ARRANGE
            var expectedCommit = await Git.CommitAsync("Commit 1");
            _ = await Git.CommitAsync("Commit 2");

            using var sut = new GitRepository(m_WorkingDirectory);

            // ACT
            var actualCommit = sut.TryGetCommit(expectedCommit.Id.Id.ToUpper());

            // ASSERT
            Assert.NotNull(actualCommit);
            Assert.Equal(expectedCommit, actualCommit);
        }
    }
}
