using System.Linq;
using System.Threading.Tasks;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Model.Text;
using Grynwald.ChangeLog.Tasks;
using Xunit;

namespace Grynwald.ChangeLog.Test.Tasks
{
    /// <summary>
    /// Tests for <see cref="AddCommitFooterTask"/>
    /// </summary>
    public class AddCommitFooterTaskTest : TestBase
    {
        [Fact]
        public async Task Run_adds_commit_footer_to_all_entries()
        {
            // ARRANGE
            var testData = new TestDataFactory();

            var changelog = new ApplicationChangeLog()
            {
                testData.GetSingleVersionChangeLog("4.5.6", entries: new[]
                {
                    testData.GetChangeLogEntry(commit: TestGitIds.Id1)
                }),
                testData.GetSingleVersionChangeLog("1.2.3", entries: new[]
                {
                    testData.GetChangeLogEntry(commit: TestGitIds.Id2, footers: new[]
                    {
                        new ChangeLogEntryFooter(new("See-Also"), new PlainTextElement("some-text"))
                    })
                })
            };

            var sut = new AddCommitFooterTask();

            // ACT
            var result = await sut.RunAsync(changelog);

            // ASSERT
            var allEntries = changelog.ChangeLogs.SelectMany(x => x.AllEntries);
            Assert.Collection(allEntries,
                entry =>
                {
                    var footer = Assert.Single(entry.Footers);
                    Assert.Equal("Commit", footer.Name.Value);
                    var commitReference = Assert.IsType<CommitReferenceTextElement>(footer.Value);
                    Assert.Equal(TestGitIds.Id1, commitReference.CommitId);
                },
                entry =>
                {
                    Assert.Equal(2, entry.Footers.Count());
                    var footer = entry.Footers.Last();
                    Assert.Equal("Commit", footer.Name.Value);
                    var commitReference = Assert.IsType<CommitReferenceTextElement>(footer.Value);
                    Assert.Equal(TestGitIds.Id2, commitReference.CommitId);
                });
        }
    }
}
