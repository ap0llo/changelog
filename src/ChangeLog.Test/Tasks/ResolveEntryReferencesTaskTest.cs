using System.Linq;
using System.Threading.Tasks;
using Grynwald.ChangeLog.ConventionalCommits;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Model.Text;
using Grynwald.ChangeLog.Pipeline;
using Grynwald.ChangeLog.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Grynwald.ChangeLog.Test.Tasks
{
    /// <summary>
    /// Tests for <see cref="ResolveEntryReferencesTask"/>
    /// </summary>
    public class ResolveEntryReferencesTaskTest
    {
        private readonly ILogger<ResolveEntryReferencesTask> m_Logger = NullLogger<ResolveEntryReferencesTask>.Instance;


        [Fact]
        public async Task Execute_replaces_commit_references_if_commit_refers_to_a_change_log_entry_in_the_same_version()
        {
            // ARRANGE
            var testData = new TestDataFactory();

            var sut = new ResolveEntryReferencesTask(m_Logger);

            var footer = new ChangeLogEntryFooter(
                new CommitMessageFooterName("footer-name"),
                new CommitReferenceTextElement("some-text", TestGitIds.Id1)
            );
            var changelog = new ApplicationChangeLog()
            {
                testData.GetSingleVersionChangeLog("1.0", entries: new[]
                {
                    testData.GetChangeLogEntry(commit: TestGitIds.Id1),
                    testData.GetChangeLogEntry(commit: TestGitIds.Id2, footers: new[] { footer })
                })
            };

            // ACT 
            var result = await sut.RunAsync(changelog);

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Success, result);
            var entryReference = Assert.IsType<ChangeLogEntryReferenceTextElement>(footer.Value);

            Assert.Equal("some-text", entryReference.Text);
            Assert.Same(changelog.Single().AllEntries.First(), entryReference.Entry);
        }

        [Fact]
        public async Task Execute_replaces_commit_references_if_commit_refers_to_a_change_log_entry_in_a_different_version()
        {
            // ARRANGE
            var testData = new TestDataFactory();

            var sut = new ResolveEntryReferencesTask(m_Logger);

            var footer = new ChangeLogEntryFooter(
                new CommitMessageFooterName("footer-name"),
                new CommitReferenceTextElement("some-text", TestGitIds.Id1)
            );
            var entry1 = testData.GetChangeLogEntry(commit: TestGitIds.Id1);
            var entry2 = testData.GetChangeLogEntry(commit: TestGitIds.Id2, footers: new[] { footer });

            var changelog = new ApplicationChangeLog()
            {
                testData.GetSingleVersionChangeLog("2.0", entries: new[] { entry1 }),
                testData.GetSingleVersionChangeLog("1.0", entries: new[] { entry2 })
            };

            // ACT 
            var result = await sut.RunAsync(changelog);

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Success, result);
            var entryReference = Assert.IsType<ChangeLogEntryReferenceTextElement>(footer.Value);

            Assert.Equal("some-text", entryReference.Text);
            Assert.Same(entry1, entryReference.Entry);
        }

        [Fact]
        public async Task Execute_does_not_replace_commit_references_if_no_entry_can_be_found()
        {
            // ARRANGE
            var testData = new TestDataFactory();

            var sut = new ResolveEntryReferencesTask(m_Logger);

            var footer = new ChangeLogEntryFooter(
                new CommitMessageFooterName("footer-name"),
                new CommitReferenceTextElement("some-text", TestGitIds.Id3)
            );
            var changelog = new ApplicationChangeLog()
            {
                testData.GetSingleVersionChangeLog("1.0", entries: new[]
                {
                    testData.GetChangeLogEntry(commit: TestGitIds.Id1),
                    testData.GetChangeLogEntry(commit: TestGitIds.Id2, footers: new[] { footer })
                })
            };

            // ACT 
            var result = await sut.RunAsync(changelog);

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Success, result);
            var commitReference = Assert.IsType<CommitReferenceTextElement>(footer.Value);

            Assert.Equal("some-text", commitReference.Text);
            Assert.Equal(TestGitIds.Id3, commitReference.CommitId);
        }

        [Fact]
        public async Task Execute_does_not_replace_commit_references_if_referenced_commit_is_current_entry()
        {
            // ARRANGE
            var testData = new TestDataFactory();

            var sut = new ResolveEntryReferencesTask(m_Logger);

            var footer = new ChangeLogEntryFooter(
                new CommitMessageFooterName("footer-name"),
                new CommitReferenceTextElement("some-text", TestGitIds.Id1)
            );
            var changelog = new ApplicationChangeLog()
            {
                testData.GetSingleVersionChangeLog("1.0", entries: new[]
                {
                    testData.GetChangeLogEntry(commit: TestGitIds.Id1, footers: new[] { footer })
                })
            };

            // ACT 
            var result = await sut.RunAsync(changelog);

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Success, result);
            var commitReference = Assert.IsType<CommitReferenceTextElement>(footer.Value);

            Assert.Equal("some-text", commitReference.Text);
            Assert.Equal(TestGitIds.Id1, commitReference.CommitId);
        }
    }
}
