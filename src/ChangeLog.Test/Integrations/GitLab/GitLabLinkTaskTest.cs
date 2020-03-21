using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using GitLabApiClient;
using GitLabApiClient.Internal.Paths;
using GitLabApiClient.Models.Commits.Responses;
using Grynwald.ChangeLog.Git;
using Grynwald.ChangeLog.Integrations.GitLab;
using Grynwald.ChangeLog.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Grynwald.ChangeLog.Test.Integrations.GitLab
{
    public class GitLabLinkTaskTest : TestBase
    {

        private readonly ILogger<GitLabLinkTask> m_Logger = NullLogger<GitLabLinkTask>.Instance;
        private readonly Mock<IGitLabClientFactory> m_ClientFactoryMock;
        private readonly Mock<IGitLabClient> m_ClientMock;
        private readonly Mock<ICommitsClient> m_CommitsClientMock;
        private readonly Mock<IGitRepository> m_RepositoryMock;

        public GitLabLinkTaskTest()
        {
            m_CommitsClientMock = new Mock<ICommitsClient>(MockBehavior.Strict);

            m_ClientMock = new Mock<IGitLabClient>(MockBehavior.Strict);
            m_ClientMock.Setup(x => x.Commits).Returns(m_CommitsClientMock.Object);

            m_ClientFactoryMock = new Mock<IGitLabClientFactory>(MockBehavior.Strict);
            m_ClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(m_ClientMock.Object);

            m_RepositoryMock = new Mock<IGitRepository>(MockBehavior.Strict);
        }

        private ProjectId MatchProjectId(string expected)
        {
            return It.Is<ProjectId>((ProjectId actual) => actual.ToString() == ((ProjectId)expected).ToString());
        }

        [Fact]
        public async Task Run_does_nothing_if_repository_does_not_have_remotes()
        {
            // ARRANGE            
            m_RepositoryMock.Setup(x => x.Remotes).Returns(Enumerable.Empty<GitRemote>());

            var sut = new GitLabLinkTask(m_Logger, m_RepositoryMock.Object, m_ClientFactoryMock.Object);
            var changeLog = new ApplicationChangeLog();

            // ACT 
            await sut.RunAsync(changeLog);

            // ASSERT
            m_ClientFactoryMock.Verify(x => x.CreateClient(It.IsAny<string>()), Times.Never);
        }

        [Theory]
        [InlineData("not-a-url")]
        [InlineData("http://not-a-gitlab-url.com")]
        public async Task Run_does_nothing_if_remote_url_cannot_be_parsed(string url)
        {
            // ARRANGE            
            m_RepositoryMock.Setup(x => x.Remotes).Returns(new[] { new GitRemote("origin", url) });

            var sut = new GitLabLinkTask(m_Logger, m_RepositoryMock.Object, m_ClientFactoryMock.Object);
            var changeLog = new ApplicationChangeLog();

            // ACT 
            await sut.RunAsync(changeLog);

            // ASSERT
            m_ClientFactoryMock.Verify(x => x.CreateClient(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Run_adds_a_link_to_all_commits_if_url_can_be_parsed()
        {
            // ARRANGE
            m_RepositoryMock.Setup(x => x.Remotes).Returns(new[] { new GitRemote("origin", "http://gitlab.com/user/repo.git") });

            m_CommitsClientMock
                .Setup(x => x.GetAsync(MatchProjectId("user/repo"), It.IsAny<string>()))
                .Returns(
                    (ProjectId id, string sha) => Task.FromResult(new Commit() { WebUrl = $"https://example.com/{sha}" })
                );

            var sut = new GitLabLinkTask(m_Logger, m_RepositoryMock.Object, m_ClientFactoryMock.Object);

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
            await sut.RunAsync(changeLog);

            // ASSERT
            var entries = changeLog.ChangeLogs.SelectMany(x => x.AllEntries).ToArray();
            Assert.All(entries, entry =>
            {
                Assert.NotNull(entry.CommitWebUri);
                var expectedUri = new Uri($"https://example.com/{entry.Commit}");
                Assert.Equal(expectedUri, entry.CommitWebUri);

                m_CommitsClientMock.Verify(x => x.GetAsync(MatchProjectId("user/repo"), entry.Commit.Id), Times.Once);
            });

            m_CommitsClientMock.Verify(x => x.GetAsync(It.IsAny<ProjectId>(), It.IsAny<string>()), Times.Exactly(entries.Length));
        }

        [Theory]
        [InlineData("gitlab.com")]
        [InlineData("example.com")]
        public async Task Run_creates_client_through_client_factory(string hostName)
        {
            // ARRANGE          
            m_RepositoryMock.Setup(x => x.Remotes).Returns(new[] { new GitRemote("origin", $"http://{hostName}/owner/repo.git") });

            var sut = new GitLabLinkTask(m_Logger, m_RepositoryMock.Object, m_ClientFactoryMock.Object);

            // ACT 
            await sut.RunAsync(new ApplicationChangeLog());

            // ASSERT
            m_ClientFactoryMock.Verify(x => x.CreateClient(It.IsAny<string>()), Times.Once);
            m_ClientFactoryMock.Verify(x => x.CreateClient(hostName), Times.Once);
        }


        //TODO: Run does not add a commit link if commit cannot be found
    }
}
