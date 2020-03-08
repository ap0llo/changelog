using ChangeLogCreator.ConventionalCommits;
using ChangeLogCreator.Model;
using Xunit;

namespace ChangeLogCreator.Test.Model
{
    public class ChangeLogEntryTest : TestBase
    {
        [Fact]
        public void ContainsBreakingChanges_is_true_when_commit_message_was_marked_as_breaking_change()
        {
            // ARRANGE
            var commitMessage = GetCommitMessage(isBreakingChange: true);

            // ACT 
            var changelogEntry = ChangeLogEntry.FromCommitMessage(GetGitCommit(), commitMessage);

            // ASSERT
            Assert.True(changelogEntry.ContainsBreakingChanges);
        }

        [Fact]
        public void ContainsBreakingChanges_is_true_when_commit_message_contains_a_breaking_change_footer()
        {
            // ARRANGE
            var commitMessage = GetCommitMessage(footers: new[] { new CommitMessageFooter(CommitMessageFooterName.BreakingChange, "some breaking change") });

            // ACT 
            var changelogEntry = ChangeLogEntry.FromCommitMessage(GetGitCommit(), commitMessage);

            // ASSERT
            Assert.True(changelogEntry.ContainsBreakingChanges);
        }

        [Fact]
        public void Footers_does_not_return_breaking_changes_footers_01()
        {
            // ARRANGE
            var commitMessage = GetCommitMessage(footers: new[] { new CommitMessageFooter(CommitMessageFooterName.BreakingChange, "some breaking change") });

            // ACT 
            var changelogEntry = ChangeLogEntry.FromCommitMessage(GetGitCommit(), commitMessage);

            // ASSERT
            Assert.Empty(changelogEntry.Footers);
        }

        [Fact]
        public void Footers_does_not_return_breaking_changes_footers_02()
        {
            // ARRANGE
            var commitMessage = GetCommitMessage(footers: new[] { new CommitMessageFooter(new CommitMessageFooterName("some-footer"), "some breaking change") });

            // ACT 
            var changelogEntry = ChangeLogEntry.FromCommitMessage(GetGitCommit(), commitMessage);

            // ASSERT
            var footer = Assert.Single(changelogEntry.Footers);
            Assert.Equal(new CommitMessageFooterName("some-footer"), footer.Name);
            Assert.Equal("some breaking change", footer.Value);
        }

        [Fact]
        public void BreakingChanges_returns_descriptions_of_breaking_changes()
        {
            // ARRANGE
            var commitMessage = GetCommitMessage(footers: new[] { new CommitMessageFooter(CommitMessageFooterName.BreakingChange, "some breaking change") });

            // ACT 
            var changelogEntry = ChangeLogEntry.FromCommitMessage(GetGitCommit(), commitMessage);

            // ASSERT
            var description = Assert.Single(changelogEntry.BreakingChangeDescriptions);
            Assert.Equal("some breaking change", description);
        }
    }
}
