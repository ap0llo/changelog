using System;
using Grynwald.ChangeLog.ConventionalCommits;
using Xunit;

namespace Grynwald.ChangeLog.Test.ConventionalCommits
{
    /// <summary>
    /// Tests for <see cref="CommitMessageFooterName"/>
    /// </summary>
    public class CommitMessageFooterNameTest
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("\t")]
        public void Key_must_not_be_null_or_whitespace(string key)
        {
            Assert.Throws<ArgumentException>(() => new CommitMessageFooterName(key));
        }

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
            Assert.Equal(left.GetHashCode(), right.GetHashCode());
            Assert.Equal(left, right);
            Assert.True(left.Equals(right));
            Assert.True(left.Equals((object)right));
            Assert.True(right.Equals(left));
            Assert.True(right.Equals((object)left));
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
            Assert.NotEqual(left.GetHashCode(), right.GetHashCode());
            Assert.NotEqual(left, right);
            Assert.False(left.Equals((object)right));
            Assert.False(right.Equals(left));
            Assert.False(right.Equals((object)left));
            Assert.False(left == right);
            Assert.True(left != right);
        }

        [Fact]
        public void Equals_returns_false_if_the_argument_is_not_a_CommitMessageFooterName()
        {
            var sut = new CommitMessageFooterName("name");
            Assert.False(sut.Equals(new object()));
        }
    }
}

