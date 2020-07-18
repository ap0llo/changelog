using System;
using System.Collections.Generic;
using Grynwald.ChangeLog.ConventionalCommits;
using Xunit;

namespace Grynwald.ChangeLog.Test.ConventionalCommits
{
    /// <summary>
    /// Tests for <see cref="CommitMessageFooter"/>
    /// </summary>
    public class CommitMessageFooterTest : EqualityTest<CommitMessageFooter, CommitMessageFooterTest>, IEqualityTestDataProvider<CommitMessageFooter>
    {
        public IEnumerable<(CommitMessageFooter left, CommitMessageFooter right)> GetEqualTestCases()
        {
            yield return (
                new CommitMessageFooter(new CommitMessageFooterName("Footer-Name"), "Value"),
                new CommitMessageFooter(new CommitMessageFooterName("Footer-Name"), "Value")
            );

            yield return (
                new CommitMessageFooter(new CommitMessageFooterName("Footer-Name"), "Value"),
                new CommitMessageFooter(new CommitMessageFooterName("footer-name"), "Value")
            );
        }

        public IEnumerable<(CommitMessageFooter left, CommitMessageFooter right)> GetUnequalTestCases()
        {
            yield return (
                new CommitMessageFooter(new CommitMessageFooterName("Footer-Name"), "Value"),
                new CommitMessageFooter(new CommitMessageFooterName("Footer-Name"), "Some other Value")
            );

            yield return (
                new CommitMessageFooter(new CommitMessageFooterName("Footer-Name"), "Value"),
                new CommitMessageFooter(new CommitMessageFooterName("Some-Other-Footer-Name"), "Value")
            );
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("\t")]
        public void Value_must_not_be_null_or_whitespace(string value)
        {
            Assert.Throws<ArgumentException>(() => new CommitMessageFooter(new CommitMessageFooterName("name"), value));
        }
    }
}
