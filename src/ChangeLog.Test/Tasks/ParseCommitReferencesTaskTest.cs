using System;
using System.Threading.Tasks;
using Grynwald.ChangeLog.ConventionalCommits;
using Grynwald.ChangeLog.Git;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Model.Text;
using Grynwald.ChangeLog.Pipeline;
using Grynwald.ChangeLog.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Grynwald.ChangeLog.Test.Tasks
{
    /// <summary>
    /// Tests for <see cref="ParseCommitReferencesTask"/>
    /// </summary>
    public class ParseCommitReferencesTaskTest
    {
        private readonly Mock<IGitRepository> m_GitRepositoryMock;
        private readonly ILogger<ParseCommitReferencesTask> m_Logger = NullLogger<ParseCommitReferencesTask>.Instance;

        public ParseCommitReferencesTaskTest()
        {
            m_GitRepositoryMock = new Mock<IGitRepository>(MockBehavior.Strict);
        }


        [Fact]
        public void Logger_must_not_be_null()
        {
            Assert.Throws<ArgumentNullException>(() => new ParseCommitReferencesTask(null!, m_GitRepositoryMock.Object));
        }

        [Fact]
        public void GitRepository_must_not_be_null()
        {
            Assert.Throws<ArgumentNullException>(() => new ParseCommitReferencesTask(m_Logger, null!));
        }

        [Theory]
        [InlineData("not-a-commit-id")]
        public async Task Executes_ignores_footer_values_that_are_not_valid_commit_ids(string footerValue)
        {
            // ARRANGE
            var footer = new ChangeLogEntryFooter(new CommitMessageFooterName("some-footer"), new PlainTextElement(footerValue));

            var sut = new ParseCommitReferencesTask(m_Logger, m_GitRepositoryMock.Object);

            // ACT 
            var result = await sut.RunAsync(GetChangeLogFromFooter(footer));

            // ASSERT
            Assert.IsType<PlainTextElement>(footer.Value);

            m_GitRepositoryMock.Verify(x => x.TryGetCommit(It.IsAny<string>()), Times.Never);
        }

        [Theory]
        [InlineData("efgh567")]
        [InlineData("abcd1234abcd1234abcd1234abcd1234abcd1234")]
        public async Task Executes_adds_a_commit_link_to_footers_if_the_value_is_a_commit_id(string footerValue)
        {
            // ARRANGE
            var footer = new ChangeLogEntryFooter(new CommitMessageFooterName("some-footer"), new PlainTextElement(footerValue));
            var expectedCommit = new TestDataFactory().GetGitCommit();

            m_GitRepositoryMock
                .Setup(x => x.TryGetCommit(footerValue))
                .Returns(expectedCommit);

            var sut = new ParseCommitReferencesTask(m_Logger, m_GitRepositoryMock.Object);

            // ACT 
            var result = await sut.RunAsync(GetChangeLogFromFooter(footer));

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Success, result);

            var commitLink = Assert.IsType<CommitReferenceTextElement>(footer.Value);
            Assert.Equal(expectedCommit.Id, commitLink.CommitId);
            Assert.Equal(footerValue, commitLink.Text);

            m_GitRepositoryMock.Verify(x => x.TryGetCommit(It.IsAny<string>()), Times.Once);
            m_GitRepositoryMock.Verify(x => x.TryGetCommit(footerValue), Times.Once);
        }

        [Theory]
        [InlineData("abcd1234", "abcd1234  ")]
        [InlineData("abcd1234", "  abcd1234")]
        public async Task Executes_ignores_leading_and_trailing_whitespace_in_footer_values(string commitId, string footerValue)
        {
            // ARRANGE
            var footer = new ChangeLogEntryFooter(new CommitMessageFooterName("some-footer"), new PlainTextElement(footerValue));
            var expectedCommit = new TestDataFactory().GetGitCommit();

            m_GitRepositoryMock
                .Setup(x => x.TryGetCommit(commitId))
                .Returns(expectedCommit);

            var sut = new ParseCommitReferencesTask(m_Logger, m_GitRepositoryMock.Object);

            // ACT 
            var result = await sut.RunAsync(GetChangeLogFromFooter(footer));

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Success, result);

            var commitLink = Assert.IsType<CommitReferenceTextElement>(footer.Value);
            Assert.Equal(expectedCommit.Id, commitLink.CommitId);
            Assert.Equal(footerValue, commitLink.Text);

            m_GitRepositoryMock.Verify(x => x.TryGetCommit(It.IsAny<string>()), Times.Once);
            m_GitRepositoryMock.Verify(x => x.TryGetCommit(commitId), Times.Once);
        }

        [Theory]
        [InlineData("efgh567")]
        [InlineData("abcd1234abcd1234abcd1234abcd1234abcd1234")]
        public async Task Executes_does_not_add_a_commit_link_if_a_link_is_already_set(string footerValue)
        {
            // ARRANGE
            var uri = new Uri("https://example.com");
            var footer = new ChangeLogEntryFooter(new CommitMessageFooterName("some-footer"), new WebLinkTextElement(footerValue, uri));
            var expectedCommit = new TestDataFactory().GetGitCommit();

            m_GitRepositoryMock
                .Setup(x => x.TryGetCommit(footerValue))
                .Returns(expectedCommit);

            var sut = new ParseCommitReferencesTask(m_Logger, m_GitRepositoryMock.Object);

            // ACT 
            var result = await sut.RunAsync(GetChangeLogFromFooter(footer));

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Success, result);

            Assert.NotNull(footer.Value);
            var webLink = Assert.IsType<WebLinkTextElement>(footer.Value);
            Assert.Equal(uri, webLink.Uri);
            Assert.Equal(footerValue, webLink.Text);

            m_GitRepositoryMock.Verify(x => x.TryGetCommit(It.IsAny<string>()), Times.Once);
            m_GitRepositoryMock.Verify(x => x.TryGetCommit(footerValue), Times.Once);
        }


        private ApplicationChangeLog GetChangeLogFromFooter(ChangeLogEntryFooter footer)
        {
            var testData = new TestDataFactory();

            return new ApplicationChangeLog()
            {
                testData.GetSingleVersionChangeLog("1.0", entries: new[]
                {
                    testData.GetChangeLogEntry(footers: new[] { footer })
                })
            };
        }
    }
}
