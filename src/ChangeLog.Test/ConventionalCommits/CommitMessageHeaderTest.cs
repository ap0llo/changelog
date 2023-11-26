using System;
using System.Collections.Generic;
using Grynwald.ChangeLog.ConventionalCommits;
using Xunit;

namespace Grynwald.ChangeLog.Test.ConventionalCommits
{
    /// <summary>
    /// Tests for <see cref="CommitMessageHeader"/>
    /// </summary>
    public class CommitMessageHeaderTest : EqualityTest<CommitMessageHeader, CommitMessageHeaderTest>, IEqualityTestDataProvider<CommitMessageHeader>
    {
        public IEnumerable<(CommitMessageHeader left, CommitMessageHeader right)> GetUnequalTestCases()
        {
            yield return (
                new CommitMessageHeader(CommitType.Feature, "Description"),
                new CommitMessageHeader(CommitType.Feature, "Some other Description")
            );

            yield return (
                new CommitMessageHeader(CommitType.Feature, "Description", "someScope"),
                new CommitMessageHeader(CommitType.Feature, "Description", "someOtherScope")
            );

            yield return (
                new CommitMessageHeader(CommitType.Feature, "Description"),
                new CommitMessageHeader(CommitType.BugFix, "Description")
            );

            yield return (
                new CommitMessageHeader(CommitType.Feature, "Description", scope: null, isBreakingChange: true),
                new CommitMessageHeader(CommitType.Feature, "Description", scope: null, isBreakingChange: false)
            );
        }

        public IEnumerable<(CommitMessageHeader left, CommitMessageHeader right)> GetEqualTestCases()
        {
            yield return (
                new CommitMessageHeader(CommitType.Feature, "Description"),
                new CommitMessageHeader(CommitType.Feature, "Description")
            );

            yield return (
                new CommitMessageHeader(CommitType.Feature, "Description", scope: null, isBreakingChange: true),
                new CommitMessageHeader(CommitType.Feature, "Description", scope: null, isBreakingChange: true)
            );

            yield return (
                new CommitMessageHeader(CommitType.Feature, "Description", "someScope"),
                new CommitMessageHeader(CommitType.Feature, "Description", "someScope")
            );

            yield return (
                new CommitMessageHeader(CommitType.Feature, "Description", "someScope", isBreakingChange: true),
                new CommitMessageHeader(CommitType.Feature, "Description", "someScope", isBreakingChange: true)
            );
        }


        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("\t")]
        public void Description_must_not_be_null_or_whitespace(string? description)
        {
            Assert.Throws<ArgumentException>(() => new CommitMessageHeader(CommitType.Feature, description!));
            Assert.Throws<ArgumentException>(() => new CommitMessageHeader(CommitType.Feature, description!, "someScope"));
        }
    }
}
