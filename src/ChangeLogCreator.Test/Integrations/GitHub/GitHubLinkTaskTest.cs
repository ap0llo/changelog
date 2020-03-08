using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChangeLogCreator.Git;
using ChangeLogCreator.Integrations.GitHub;
using ChangeLogCreator.Model;
using Moq;
using Xunit;

namespace ChangeLogCreator.Test.Integrations.GitHub
{
    public class GitHubLinkTaskTest : TestBase
    {
        [Fact]
        public async Task Run_does_nothing_if_repository_does_not_have_remotes()
        {
            // ARRANGE
            var repoMock = new Mock<IGitRepository>(MockBehavior.Strict);
            repoMock.Setup(x => x.Remotes).Returns(Enumerable.Empty<GitRemote>());

            var sut = new GitHubLinkTask(repoMock.Object);
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

            var sut = new GitHubLinkTask(repoMock.Object);
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

            var sut = new GitHubLinkTask(repoMock.Object);

            var changeLog = new ChangeLog()
            {
                GetSingleVersionChangeLog(
                    "1.2.3",
                    null,
                    GetChangeLogEntry(summary: "Entry1"),
                    GetChangeLogEntry(summary: "Entry2")
                ),
                GetSingleVersionChangeLog(
                    "4.5.6",
                    null,
                    GetChangeLogEntry(summary: "Entry1"),
                    GetChangeLogEntry(summary: "Entry2")
                )
            };

            // ACT 
            await sut.RunAsync(changeLog);

            // ASSERT
            var entries = changeLog.ChangeLogs.SelectMany(x => x.AllEntries);
            Assert.All(entries, entry =>
            {
                Assert.NotNull(entry.CommitWebUri);
                var expectedUri = new Uri($"https://github.com/owner/repo/commit/{entry.Commit}");
                Assert.Equal(expectedUri, entry.CommitWebUri);
            });
        }

    }
}
