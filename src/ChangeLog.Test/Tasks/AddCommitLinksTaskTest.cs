using System;
using System.Linq;
using System.Threading.Tasks;
using Grynwald.ChangeLog.ConventionalCommits;
using Grynwald.ChangeLog.Git;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Grynwald.ChangeLog.Test.Tasks
{
    /// <summary>
    /// Tests for <see cref="AddCommitLinksTask"/>
    /// </summary>
    public class AddCommitLinksTaskTest
    {
        private readonly Mock<IGitRepository> m_GitRepositoryMock;
        private readonly ILogger<AddCommitLinksTask> m_Logger = NullLogger<AddCommitLinksTask>.Instance;

        public AddCommitLinksTaskTest()
        {
            m_GitRepositoryMock = new Mock<IGitRepository>(MockBehavior.Strict);
        }


        [Fact]
        public void Logger_must_not_be_null()
        {
            Assert.Throws<ArgumentNullException>(() => new AddCommitLinksTask(null!, m_GitRepositoryMock.Object));
        }

        [Fact]
        public void GitRepository_must_not_be_null()
        {
            Assert.Throws<ArgumentNullException>(() => new AddCommitLinksTask(m_Logger, null!));
        }

        [Theory]
        [InlineData("not-a-commit-id")]
        public async Task Executes_ignores_footer_values_that_are_not_valid_commit_ids(string footerValue)
        {
            // ARRANGE
            var testData = new TestDataFactory();

            var changelog = new ApplicationChangeLog()
            {
                testData.GetSingleVersionChangeLog("1.0", entries: new[]
                {
                    testData.GetChangeLogEntry(footers: new[]
                    {
                        new ChangeLogEntryFooter(new CommitMessageFooterName("some-footer"), footerValue)
                    })
                })
            };

            var sut = new AddCommitLinksTask(m_Logger, m_GitRepositoryMock.Object);

            // ACT 
            var result = await sut.RunAsync(changelog);

            // ASSERT
            var footers = changelog.ChangeLogs.SelectMany(x => x.AllEntries).SelectMany(x => x.Footers);
            var footer = Assert.Single(footers);
            Assert.Null(footer.Link);

            m_GitRepositoryMock.Verify(x => x.TryGetCommit(It.IsAny<string>()), Times.Never);
        }

        [Theory]
        [InlineData("efgh567")]
        [InlineData("abcd1234abcd1234abcd1234abcd1234abcd1234")]
        public async Task Executes_adds_a_commit_link_to_footers_if_the_value_is_a_commit_id(string footerValue)
        {
            // ARRANGE
            var testData = new TestDataFactory();

            var changelog = new ApplicationChangeLog()
            {
                testData.GetSingleVersionChangeLog("1.0", entries: new[]
                {
                    testData.GetChangeLogEntry(footers: new[]
                    {
                        new ChangeLogEntryFooter(new CommitMessageFooterName("some-footer"), footerValue)
                    })
                })
            };

            var expectedCommit = testData.GetGitCommit();

            m_GitRepositoryMock
                .Setup(x => x.TryGetCommit(footerValue))
                .Returns(expectedCommit);

            var sut = new AddCommitLinksTask(m_Logger, m_GitRepositoryMock.Object);

            // ACT 
            var result = await sut.RunAsync(changelog);

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Success, result);

            var footers = changelog.ChangeLogs.SelectMany(x => x.AllEntries).SelectMany(x => x.Footers);
            var footer = Assert.Single(footers);

            Assert.NotNull(footer.Link);
            var commitLink = Assert.IsType<CommitLink>(footer.Link);
            Assert.Equal(expectedCommit.Id, commitLink.Id);

            m_GitRepositoryMock.Verify(x => x.TryGetCommit(It.IsAny<string>()), Times.Once);
            m_GitRepositoryMock.Verify(x => x.TryGetCommit(footerValue), Times.Once);
        }

        [Theory]
        [InlineData("efgh567")]
        [InlineData("abcd1234abcd1234abcd1234abcd1234abcd1234")]
        public async Task Executes_does_not_add_a_commit_link_if_a_link_is_already_set(string footerValue)
        {
            // ARRANGE
            var uri = new Uri("https://example.com");
            var testData = new TestDataFactory();

            var changelog = new ApplicationChangeLog()
            {
                testData.GetSingleVersionChangeLog("1.0", entries: new[]
                {
                    testData.GetChangeLogEntry(footers: new[]
                    {
                        new ChangeLogEntryFooter(new CommitMessageFooterName("some-footer"), footerValue)
                        {
                            Link = new WebLink(uri)
                        }
                    })
                })
            };

            m_GitRepositoryMock
                .Setup(x => x.TryGetCommit(footerValue))
                .Returns(() => testData.GetGitCommit());

            var sut = new AddCommitLinksTask(m_Logger, m_GitRepositoryMock.Object);

            // ACT 
            var result = await sut.RunAsync(changelog);

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Success, result);

            var footers = changelog.ChangeLogs.SelectMany(x => x.AllEntries).SelectMany(x => x.Footers);
            var footer = Assert.Single(footers);

            Assert.NotNull(footer.Link);
            var webLink = Assert.IsType<WebLink>(footer.Link);
            Assert.Equal(uri, webLink.Uri);

            m_GitRepositoryMock.Verify(x => x.TryGetCommit(It.IsAny<string>()), Times.Once);
            m_GitRepositoryMock.Verify(x => x.TryGetCommit(footerValue), Times.Once);
        }
    }
}
