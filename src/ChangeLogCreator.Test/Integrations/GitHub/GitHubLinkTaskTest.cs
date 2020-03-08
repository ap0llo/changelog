using System;
using System.Linq;
using System.Threading.Tasks;
using ChangeLogCreator.Git;
using ChangeLogCreator.Integrations.GitHub;
using ChangeLogCreator.Model;
using Moq;
using Octokit;
using Xunit;

namespace ChangeLogCreator.Test.Integrations.GitHub
{
    public class GitHubLinkTaskTest : TestBase
    {
        private class TestGitHubCommit : GitHubCommit
        {
            public TestGitHubCommit(string htmlUrl)
            {
                HtmlUrl = htmlUrl;
            }
        }

        private readonly Mock<IGitHubClient> m_GithubClientMock;
        private readonly Mock<IRepositoryCommitsClient> m_GitHubCommitsClientMock;
        private readonly Mock<IRepositoriesClient> m_GitHubRepositoriesClientMock;

        public GitHubLinkTaskTest()
        {
            m_GitHubCommitsClientMock = new Mock<IRepositoryCommitsClient>(MockBehavior.Strict);

            m_GitHubRepositoriesClientMock = new Mock<IRepositoriesClient>(MockBehavior.Strict);
            m_GitHubRepositoriesClientMock.Setup(x => x.Commit).Returns(m_GitHubCommitsClientMock.Object);

            m_GithubClientMock = new Mock<IGitHubClient>(MockBehavior.Strict);
            m_GithubClientMock.Setup(x => x.Repository).Returns(m_GitHubRepositoriesClientMock.Object);
        }

        [Fact]
        public async Task Run_does_nothing_if_repository_does_not_have_remotes()
        {
            // ARRANGE
            var repoMock = new Mock<IGitRepository>(MockBehavior.Strict);
            repoMock.Setup(x => x.Remotes).Returns(Enumerable.Empty<GitRemote>());

            var sut = new GitHubLinkTask(repoMock.Object, m_GithubClientMock.Object);
            var changeLog = new ChangeLog();

            // ACT 
            await sut.RunAsync(changeLog);

            // ASSERT
        }

        [Theory]
        [InlineData("not-a-url")]
        [InlineData("http://not-a-github-url.com")]
        public async Task Run_does_nothing_if_remote_url_cannot_be_parsed(string url)
        {
            // ARRANGE
            var repoMock = new Mock<IGitRepository>(MockBehavior.Strict);
            repoMock.Setup(x => x.Remotes).Returns(new[] { new GitRemote("origin", url) });

            var sut = new GitHubLinkTask(repoMock.Object, m_GithubClientMock.Object);
            var changeLog = new ChangeLog();

            // ACT 
            await sut.RunAsync(changeLog);

            // ASSERT
        }

        [Fact]
        public async Task Run_adds_a_link_to_all_commits_if_url_can_be_parsed()
        {
            // ARRANGE
            var repoMock = new Mock<IGitRepository>(MockBehavior.Strict);
            repoMock.Setup(x => x.Remotes).Returns(new[] { new GitRemote("origin", "http://github.com/owner/repo.git") });

            m_GitHubCommitsClientMock
                .Setup(x => x.Get("owner", "repo", It.IsAny<string>()))
                .Returns(
                    (string owner, string repo, string sha) => Task.FromResult<GitHubCommit>(new TestGitHubCommit($"https://example.com/{sha}"))
                );

            var sut = new GitHubLinkTask(repoMock.Object, m_GithubClientMock.Object);

            var changeLog = new ChangeLog()
            {
                GetSingleVersionChangeLog(
                    "1.2.3",
                    null,
                    GetChangeLogEntry(summary: "Entry1", commit: "01"),
                    GetChangeLogEntry(summary: "Entry2", commit: "02")
                ),
                GetSingleVersionChangeLog(
                    "4.5.6",
                    null,
                    GetChangeLogEntry(summary: "Entry1", commit: "03"),
                    GetChangeLogEntry(summary: "Entry2", commit: "04")
                )
            };

            // ACT 
            await sut.RunAsync(changeLog);

            // ASSERT
            var entries = changeLog.ChangeLogs.SelectMany(x => x.AllEntries).ToArray();
            Assert.All(entries, entry =>
            {
                Assert.NotNull(entry.CommitWebUri);
                var expectedUri = new Uri($"https://example.com/{entry.Commit}");
                Assert.Equal(expectedUri, entry.CommitWebUri);

                m_GitHubCommitsClientMock.Verify(x => x.Get("owner", "repo", entry.Commit.Id), Times.Once);
            });

            m_GitHubCommitsClientMock.Verify(x => x.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(entries.Length));
        }
    }
}
