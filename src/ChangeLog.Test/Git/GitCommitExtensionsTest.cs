using Grynwald.ChangeLog.Git;
using Xunit;

namespace Grynwald.ChangeLog.Test.Git
{
    /// <summary>
    /// Tests for <see cref="GitCommitExtensions"/>
    /// </summary>
    public class GitCommitExtensionsTest : TestBase
    {
        [Fact]
        public void WithCommitMessage_replaces_a_commits_message()
        {
            // ARRANGE
            var author = new GitAuthor("Someone", "someone@example.com");
            var originalCommit = new GitCommit(NextGitId(), "original message", NextCommitDate(), author);

            // ACT 
            var modifiedCommit = originalCommit.WithCommitMessage("new message");

            // ASSERT
            Assert.NotEqual(originalCommit, modifiedCommit);
            Assert.Equal(originalCommit.Id, modifiedCommit.Id);
            Assert.Equal("new message", modifiedCommit.CommitMessage);
            Assert.Equal(originalCommit.Date, modifiedCommit.Date);
            Assert.Equal(originalCommit.Author, modifiedCommit.Author);
        }
    }
}
