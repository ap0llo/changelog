using System;
using Grynwald.ChangeLog.ConventionalCommits;
using Grynwald.ChangeLog.Model;
using Xunit;

namespace Grynwald.ChangeLog.Test.Model
{
    /// <summary>
    /// Tests for <see cref="ChangeLogEntryFooter"/>
    /// </summary>
    public class ChangeLogEntryFooterTest
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("\t")]
        public void Value_must_not_be_null_or_whitespace(string footerValue)
        {
            Assert.Throws<ArgumentException>(() => new ChangeLogEntryFooter(new CommitMessageFooterName("irrelevant"), footerValue));
        }

        [Fact]
        public void Constructor_initializes_properties()
        {
            // ARRANGE
            var name = new CommitMessageFooterName("Some-Name");
            var value = "some value";

            // ACT
            var sut = new ChangeLogEntryFooter(name, value);

            // ASSERT
            Assert.Equal(name, sut.Name);
            Assert.Equal(value, sut.Value);
        }

        [Fact]
        public void FromCommitMessageFooter_returns_expected_footer()
        {
            // ARRANGE
            var name = new CommitMessageFooterName("Some-Name");
            var value = "some value";
            var commitMessageFooter = new CommitMessageFooter(name, value);

            // ACT
            var sut = ChangeLogEntryFooter.FromCommitMessageFooter(commitMessageFooter);

            // ASSERT
            Assert.Equal(name, sut.Name);
            Assert.Equal(value, sut.Value);
        }
    }
}
