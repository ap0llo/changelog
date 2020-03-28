using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Grynwald.ChangeLog.ConventionalCommits;
using Grynwald.ChangeLog.Git;
using Grynwald.ChangeLog.Integrations.GitHub;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Octokit;
using Xunit;

namespace Grynwald.ChangeLog.Test.Integrations.GitHub
{
    /// <summary>
    /// Unit tests for <see cref="GitHubLinkTask"/>
    /// </summary>
    public class GitHubLinkTaskTest : TestBase
    {
        private class TestGitHubCommit : GitHubCommit
        {
            public TestGitHubCommit(string htmlUrl)
            {
                HtmlUrl = htmlUrl;
            }
        }

        private class TestGitHubIssue : Issue
        {
            public TestGitHubIssue(string htmlUrl)
            {
                HtmlUrl = htmlUrl;
            }
        }

        private class TestGitHubPullRequest : PullRequest
        {
            public TestGitHubPullRequest(string htmlUrl)
            {
                HtmlUrl = htmlUrl;
            }
        }

        private readonly ILogger<GitHubLinkTask> m_Logger = NullLogger<GitHubLinkTask>.Instance;
        private readonly Mock<IGitHubClient> m_GithubClientMock;
        private readonly Mock<IRepositoryCommitsClient> m_GitHubCommitsClientMock;
        private readonly Mock<IRepositoriesClient> m_GitHubRepositoriesClientMock;
        private readonly Mock<IIssuesClient> m_GitHubIssuesClientMock;
        private readonly Mock<IPullRequestsClient> m_GitHubPullRequestsClient;
        private readonly Mock<IMiscellaneousClient> m_GitHubMiscellaneousClientMock;
        private readonly Mock<IGitHubClientFactory> m_GitHubClientFactoryMock;

        public GitHubLinkTaskTest()
        {
            m_GitHubCommitsClientMock = new Mock<IRepositoryCommitsClient>(MockBehavior.Strict);

            m_GitHubRepositoriesClientMock = new Mock<IRepositoriesClient>(MockBehavior.Strict);
            m_GitHubRepositoriesClientMock.Setup(x => x.Commit).Returns(m_GitHubCommitsClientMock.Object);

            m_GitHubIssuesClientMock = new Mock<IIssuesClient>(MockBehavior.Strict);

            m_GitHubPullRequestsClient = new Mock<IPullRequestsClient>(MockBehavior.Strict);

            m_GitHubMiscellaneousClientMock = new Mock<IMiscellaneousClient>(MockBehavior.Strict);
            m_GitHubMiscellaneousClientMock
                .Setup(x => x.GetRateLimits())
                .Returns(Task.FromResult(new MiscellaneousRateLimit(new ResourceRateLimit(), new RateLimit())));

            m_GithubClientMock = new Mock<IGitHubClient>(MockBehavior.Strict);
            m_GithubClientMock.Setup(x => x.Repository).Returns(m_GitHubRepositoriesClientMock.Object);
            m_GithubClientMock.Setup(x => x.Issue).Returns(m_GitHubIssuesClientMock.Object);
            m_GithubClientMock.Setup(x => x.PullRequest).Returns(m_GitHubPullRequestsClient.Object);
            m_GithubClientMock.Setup(x => x.Miscellaneous).Returns(m_GitHubMiscellaneousClientMock.Object);

            m_GitHubClientFactoryMock = new Mock<IGitHubClientFactory>(MockBehavior.Strict);
            m_GitHubClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(m_GithubClientMock.Object);
        }

        [Fact]
        public async Task Run_does_nothing_if_repository_does_not_have_remotes()
        {
            // ARRANGE
            var repoMock = new Mock<IGitRepository>(MockBehavior.Strict);
            repoMock.Setup(x => x.Remotes).Returns(Enumerable.Empty<GitRemote>());

            var sut = new GitHubLinkTask(m_Logger, repoMock.Object, m_GitHubClientFactoryMock.Object);
            var changeLog = new ApplicationChangeLog();

            // ACT 
            var result = await sut.RunAsync(changeLog);

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Skipped, result);
            m_GitHubClientFactoryMock.Verify(x => x.CreateClient(It.IsAny<string>()), Times.Never);
        }

        [Theory]
        [InlineData("not-a-url")]
        [InlineData("http://not-a-github-url.com")]
        public async Task Run_does_nothing_if_remote_url_cannot_be_parsed(string url)
        {
            // ARRANGE
            var repoMock = new Mock<IGitRepository>(MockBehavior.Strict);
            repoMock.Setup(x => x.Remotes).Returns(new[] { new GitRemote("origin", url) });

            var sut = new GitHubLinkTask(m_Logger, repoMock.Object, m_GitHubClientFactoryMock.Object);
            var changeLog = new ApplicationChangeLog();

            // ACT 
            var result = await sut.RunAsync(changeLog);

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Skipped, result);
            m_GitHubClientFactoryMock.Verify(x => x.CreateClient(It.IsAny<string>()), Times.Never);
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

            var sut = new GitHubLinkTask(m_Logger, repoMock.Object, m_GitHubClientFactoryMock.Object);

            var changeLog = new ApplicationChangeLog()
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
            var result = await sut.RunAsync(changeLog);

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Success, result);
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

        //TODO: Run does not add a commit link if commit cannot be found

        [Theory]
        [InlineData("#23", 23, "owner", "repo")]
        [InlineData("GH-23", 23, "owner", "repo")]
        [InlineData("anotherOwner/anotherRepo#42", 42, "anotherOwner", "anotherRepo")]
        [InlineData("another-Owner/another-Repo#42", 42, "another-Owner", "another-Repo")]
        [InlineData("another.Owner/another.Repo#42", 42, "another.Owner", "another.Repo")]
        [InlineData("another_Owner/another_Repo#42", 42, "another_Owner", "another_Repo")]
        public async Task Run_adds_issue_links_to_footers(string footerText, int issueNumber, string owner, string repo)
        {
            // ARRANGE
            var repoMock = new Mock<IGitRepository>(MockBehavior.Strict);
            repoMock.Setup(x => x.Remotes).Returns(new[] { new GitRemote("origin", "http://github.com/owner/repo.git") });

            m_GitHubCommitsClientMock
                .Setup(x => x.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(
                    (string owner, string repo, string sha) => Task.FromResult<GitHubCommit>(new TestGitHubCommit($"https://example.com/{sha}"))
                );
            m_GitHubIssuesClientMock
                .Setup(x => x.Get(owner, repo, issueNumber))
                .Returns(
                    Task.FromResult<Issue>(new TestGitHubIssue($"https://example.com/issue/{issueNumber}"))
                );

            var sut = new GitHubLinkTask(m_Logger, repoMock.Object, m_GitHubClientFactoryMock.Object);

            var changeLog = new ApplicationChangeLog()
            {
                GetSingleVersionChangeLog(
                    "1.2.3",
                    null,
                    GetChangeLogEntry(summary: "Entry1", commit: "01", footers: new []
                    {
                        new ChangeLogEntryFooter(new CommitMessageFooterName("Issue"), footerText)
                    })
                )
            };

            // ACT 
            var result = await sut.RunAsync(changeLog);

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Success, result);

            var entries = changeLog.ChangeLogs.SelectMany(x => x.AllEntries).ToArray();
            Assert.All(entries, entry =>
            {
                Assert.All(entry.Footers.Where(x => x.Name == new CommitMessageFooterName("Issue")), footer =>
                {
                    Assert.NotNull(footer.WebUri);
                    var expectedUri = new Uri($"https://example.com/issue/{issueNumber}");
                    Assert.Equal(expectedUri, footer.WebUri);
                });

            });

            m_GitHubIssuesClientMock.Verify(x => x.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Once);
        }

        [Theory]
        [InlineData("#23", 23, "owner", "repo")]
        [InlineData("GH-23", 23, "owner", "repo")]
        [InlineData("anotherOwner/anotherRepo#42", 42, "anotherOwner", "anotherRepo")]
        [InlineData("another-Owner/another-Repo#42", 42, "another-Owner", "another-Repo")]
        [InlineData("another.Owner/another.Repo#42", 42, "another.Owner", "another.Repo")]
        [InlineData("another_Owner/another_Repo#42", 42, "another_Owner", "another_Repo")]
        // Linking must ignore trailing and leading whitespace
        [InlineData(" #23", 23, "owner", "repo")]
        [InlineData("#23 ", 23, "owner", "repo")]
        [InlineData(" GH-23", 23, "owner", "repo")]
        [InlineData("GH-23 ", 23, "owner", "repo")]
        [InlineData(" anotherOwner/anotherRepo#42", 42, "anotherOwner", "anotherRepo")]
        [InlineData("anotherOwner/anotherRepo#42  ", 42, "anotherOwner", "anotherRepo")]
        public async Task Run_adds_pull_request_links_to_footers(string footerText, int prNumber, string owner, string repo)
        {
            // ARRANGE
            var repoMock = new Mock<IGitRepository>(MockBehavior.Strict);
            repoMock.Setup(x => x.Remotes).Returns(new[] { new GitRemote("origin", "http://github.com/owner/repo.git") });

            m_GitHubCommitsClientMock
                .Setup(x => x.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(
                    (string owner, string repo, string sha) => Task.FromResult<GitHubCommit>(new TestGitHubCommit($"https://example.com/{sha}"))
                );
            m_GitHubIssuesClientMock
                .Setup(x => x.Get(owner, repo, prNumber))
                .ThrowsAsync(new NotFoundException("Message", HttpStatusCode.NotFound));

            m_GitHubPullRequestsClient
                .Setup(x => x.Get(owner, repo, prNumber))
                .Returns(
                    Task.FromResult<PullRequest>(new TestGitHubPullRequest($"https://example.com/pr/{prNumber}"))
                );


            var sut = new GitHubLinkTask(m_Logger, repoMock.Object, m_GitHubClientFactoryMock.Object);

            var changeLog = new ApplicationChangeLog()
            {
                GetSingleVersionChangeLog(
                    "1.2.3",
                    null,
                    GetChangeLogEntry(summary: "Entry1", commit: "01", footers: new []
                    {
                        new ChangeLogEntryFooter(new CommitMessageFooterName("Issue"), footerText)
                    })
                )
            };

            // ACT 
            var result = await sut.RunAsync(changeLog);

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Success, result);

            var entries = changeLog.ChangeLogs.SelectMany(x => x.AllEntries).ToArray();
            Assert.All(entries, entry =>
            {
                Assert.All(entry.Footers.Where(x => x.Name == new CommitMessageFooterName("Issue")), footer =>
                {
                    Assert.NotNull(footer.WebUri);
                    var expectedUri = new Uri($"https://example.com/pr/{prNumber}");
                    Assert.Equal(expectedUri, footer.WebUri);
                });

            });

            m_GitHubPullRequestsClient.Verify(x => x.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Once);
            m_GitHubIssuesClientMock.Verify(x => x.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Once);
        }

        [Theory]
        [InlineData("not-a-reference")]
        [InlineData("Not a/reference#0")]
        [InlineData("Not a/reference#xyz")]
        [InlineData("#xyz")]
        [InlineData("GH-xyz")]
        [InlineData("#1 2 3")]
        [InlineData("GH-1 2 3")]
        public async Task Run_ignores_footers_which_cannot_be_parsed(string footerText)
        {
            // ARRANGE
            var repoMock = new Mock<IGitRepository>(MockBehavior.Strict);
            repoMock.Setup(x => x.Remotes).Returns(new[] { new GitRemote("origin", "http://github.com/owner/repo.git") });

            m_GitHubCommitsClientMock
                .Setup(x => x.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(
                    (string owner, string repo, string sha) => Task.FromResult<GitHubCommit>(new TestGitHubCommit($"https://example.com/{sha}"))
                );

            var sut = new GitHubLinkTask(m_Logger, repoMock.Object, m_GitHubClientFactoryMock.Object);

            var changeLog = new ApplicationChangeLog()
            {
                GetSingleVersionChangeLog(
                    "1.2.3",
                    null,
                    GetChangeLogEntry(summary: "Entry1", commit: "01", footers: new []
                    {
                        new ChangeLogEntryFooter(new CommitMessageFooterName("Irrelevant"), footerText),
                    })
                )
            };

            // ACT 
            var result = await sut.RunAsync(changeLog);

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Success, result);

            var entries = changeLog.ChangeLogs.SelectMany(x => x.AllEntries).ToArray();
            Assert.All(entries, entry =>
            {
                Assert.All(entry.Footers, footer =>
                {
                    Assert.Null(footer.WebUri);
                });

            });

            m_GitHubIssuesClientMock.Verify(x => x.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
            m_GitHubPullRequestsClient.Verify(x => x.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        }

        [Theory]
        [InlineData("#23", "owner", "repo")]
        [InlineData("GH-23", "owner", "repo")]
        [InlineData("anotherOwner/anotherRepo#23", "anotherOwner", "anotherRepo")]
        public async Task Run_does_not_add_a_links_to_footers_if_no_issue_or_pull_request_cannot_be_found(string footerText, string owner, string repo)
        {
            // ARRANGE
            var repoMock = new Mock<IGitRepository>(MockBehavior.Strict);
            repoMock.Setup(x => x.Remotes).Returns(new[] { new GitRemote("origin", "http://github.com/owner/repo.git") });

            m_GitHubCommitsClientMock
                .Setup(x => x.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(
                    (string owner, string repo, string sha) => Task.FromResult<GitHubCommit>(new TestGitHubCommit($"https://example.com/{sha}"))
                );
            m_GitHubIssuesClientMock
                .Setup(x => x.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ThrowsAsync(new NotFoundException("Not found", HttpStatusCode.NotFound));

            m_GitHubPullRequestsClient
                .Setup(x => x.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ThrowsAsync(new NotFoundException("Not found", HttpStatusCode.NotFound));

            var sut = new GitHubLinkTask(m_Logger, repoMock.Object, m_GitHubClientFactoryMock.Object);

            var changeLog = new ApplicationChangeLog()
            {
                GetSingleVersionChangeLog(
                    "1.2.3",
                    null,
                    GetChangeLogEntry(summary: "Entry1", commit: "01", footers: new []
                    {
                        new ChangeLogEntryFooter(new CommitMessageFooterName("Irrelevant"), footerText),
                    })
                )
            };

            // ACT 
            var result = await sut.RunAsync(changeLog);

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Success, result);

            var entries = changeLog.ChangeLogs.SelectMany(x => x.AllEntries).ToArray();
            Assert.All(entries, entry =>
            {
                Assert.All(entry.Footers, footer =>
                {
                    Assert.Null(footer.WebUri);
                });

            });

            m_GitHubIssuesClientMock.Verify(x => x.Get(owner, repo, It.IsAny<int>()), Times.Once);
            m_GitHubPullRequestsClient.Verify(x => x.Get(owner, repo, It.IsAny<int>()), Times.Once);
        }

        [Theory]
        [InlineData("github.com")]
        [InlineData("github.example.com")]
        [InlineData("some-domain.com")]
        public async Task Run_creates_client_through_client_factory(string hostName)
        {
            // ARRANGE
            var repoMock = new Mock<IGitRepository>(MockBehavior.Strict);
            repoMock.Setup(x => x.Remotes).Returns(new[] { new GitRemote("origin", $"http://{hostName}/owner/repo.git") });

            var sut = new GitHubLinkTask(m_Logger, repoMock.Object, m_GitHubClientFactoryMock.Object);

            // ACT 
            var result = await sut.RunAsync(new ApplicationChangeLog());

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Success, result);

            m_GitHubClientFactoryMock.Verify(x => x.CreateClient(It.IsAny<string>()), Times.Once);
            m_GitHubClientFactoryMock.Verify(x => x.CreateClient(hostName), Times.Once);
        }

    }
}
