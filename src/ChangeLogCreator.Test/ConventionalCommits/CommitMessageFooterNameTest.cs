using ChangeLogCreator.ConventionalCommits;
using Xunit;

namespace ChangeLogCreator.Test.ConventionalCommits
{
    /// <summary>
    /// Tests for <see cref="CommitMessageFooterName"/>
    /// </summary>
    public class CommitMessageFooterNameTest
    {
        [Theory]
        // Comparisons of footer types must be case-insensitive
        [InlineData("fixes", "fixes")]
        [InlineData("fixes", "Fixes")]
        [InlineData("reviewed", "Reviewed")]
        [InlineData("breaking-change", "BREAKING-CHANGE")]
        // The conventional commits specification has a special definition for breaking changes
        // and "BREAKING CHANGE" is the only footer key that may contain a space (and must be upper case)
        // The upper-case "BREAKING CHANGE" is considered equal to "breaking-change"
        [InlineData("breaking-change", "BREAKING CHANGE")]
        [InlineData("BREAKING-CHANGE", "BREAKING CHANGE")]
        public void Instances_are_equal_if_they_have_the_same_value(string leftValue, string rightValue)
        {
            // ARRANGE
            var left = new CommitMessageFooterName(leftValue);
            var right = new CommitMessageFooterName(rightValue);

            // ACT / ASSERT
            Assert.Equal(left, right);
            Assert.Equal(left.GetHashCode(), right.GetHashCode());
            Assert.True(left == right);
            Assert.False(left != right);
        }

        [Theory]
        [InlineData("fixes", "closes")]
        [InlineData("reviewed", "Reviewed-By")]
        // The conventional commits specification has a special definition for breaking changes
        // and "BREAKING CHANGE" is the only footer key that may contain a space (but must be upper case)
        [InlineData("breaking-change", "Breaking Change")]
        public void Instances_are_not_equal_if_they_have_the_different_values(string leftValue, string rightValue)
        {
            // ARRANGE
            var left = new CommitMessageFooterName(leftValue);
            var right = new CommitMessageFooterName(rightValue);

            // ACT / ASSERT
            Assert.NotEqual(left, right);
            Assert.NotEqual(left.GetHashCode(), right.GetHashCode());
            Assert.False(left == right);
            Assert.True(left != right);
        }
    }
}

