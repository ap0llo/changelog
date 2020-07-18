using System;
using System.Collections.Generic;
using Grynwald.ChangeLog.ConventionalCommits;
using Xunit;

namespace Grynwald.ChangeLog.Test.ConventionalCommits
{
    /// <summary>
    /// Tests for <see cref="CommitMessageFooterName"/>
    /// </summary>
    public class CommitMessageFooterNameTest : EqualityTest<CommitMessageFooterName, CommitMessageFooterNameTest>, IEqualityTestDataProvider<CommitMessageFooterName>
    {
        public IEnumerable<(CommitMessageFooterName left, CommitMessageFooterName right)> GetEqualTestCases()
        {
            yield return (
                new CommitMessageFooterName("fixes"),
                new CommitMessageFooterName("Fixes")
            );
            yield return (
                new CommitMessageFooterName("reviewed"),
                new CommitMessageFooterName("Reviewed")
            );
            yield return (
                new CommitMessageFooterName("breaking-change"),
                new CommitMessageFooterName("BREAKING-CHANGE")
            );

            // The conventional commits specification has a special definition for breaking changes
            // and "BREAKING CHANGE" is the only footer key that may contain a space (and must be upper case)
            // The upper-case "BREAKING CHANGE" is considered equal to "breaking-change"
            yield return (
                new CommitMessageFooterName("breaking-change"),
                new CommitMessageFooterName("BREAKING CHANGE")
            );
            yield return (
                new CommitMessageFooterName("BREAKING-CHANGE"),
                new CommitMessageFooterName("BREAKING CHANGE")
            );
        }

        public IEnumerable<(CommitMessageFooterName left, CommitMessageFooterName right)> GetUnequalTestCases()
        {
            yield return (
                new CommitMessageFooterName("fixes"),
                new CommitMessageFooterName("closes")
            );
            yield return (
                new CommitMessageFooterName("reviewed"),
                new CommitMessageFooterName("Reviewed-By")
            );

            // The conventional commits specification has a special definition for breaking changes
            // and "BREAKING CHANGE" is the only footer key that may contain a space (but must be upper case)
            yield return (
                new CommitMessageFooterName("breaking-change"),
                new CommitMessageFooterName("Breaking Change")
            );
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("\t")]
        public void Key_must_not_be_null_or_whitespace(string key)
        {
            Assert.Throws<ArgumentException>(() => new CommitMessageFooterName(key));
        }
    }
}

