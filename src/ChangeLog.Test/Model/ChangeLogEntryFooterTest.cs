using System;
using Grynwald.ChangeLog.ConventionalCommits;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Model.Text;
using Xunit;

namespace Grynwald.ChangeLog.Test.Model
{
    /// <summary>
    /// Tests for <see cref="ChangeLogEntryFooter"/>
    /// </summary>
    public class ChangeLogEntryFooterTest
    {
        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("\t")]
        public void Value_must_not_be_null_or_whitespace(string footerValue)
        {
            Assert.Throws<ArgumentException>(() => new ChangeLogEntryFooter(new CommitMessageFooterName("irrelevant"), new PlainTextElement(footerValue)));
            Assert.Throws<ArgumentException>(() => new ChangeLogEntryFooter(new CommitMessageFooterName("irrelevant"), null!));
        }

        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("\t")]
        public void Value_cannot_be_set_to_null_or_a_whitespace_value(string footerValue)
        {
            var sut = new ChangeLogEntryFooter(
                new CommitMessageFooterName("irrelevant"),
                new PlainTextElement("Some Text")
            );

            Assert.Throws<ArgumentException>(() => sut.Value = null!);
            Assert.Throws<ArgumentException>(() => sut.Value = new PlainTextElement(footerValue));
        }


        [Fact]
        public void Name_must_not_be_empty()
        {
            // ARRANGE

            // ACT
            var ex = Record.Exception(() => new ChangeLogEntryFooter(default, new PlainTextElement("Value")));

            // ASSERT
            Assert.NotNull(ex);
            Assert.IsType<ArgumentException>(ex);
        }

        [Fact]
        public void Constructor_initializes_properties()
        {
            // ARRANGE
            var name = new CommitMessageFooterName("Some-Name");
            var value = new PlainTextElement("some value");

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
            Assert.Equal(value, sut.Value.Text);
        }
    }
}
