using System;
using System.Collections.Generic;
using Grynwald.ChangeLog.ConventionalCommits;
using Xunit;

namespace Grynwald.ChangeLog.Test.ConventionalCommits
{
    /// <summary>
    /// Tests for <see cref="CommitType"/>
    /// </summary>
    public class CommitTypeTest : EqualityTest<CommitType, CommitTypeTest>, IEqualityTestDataProvider<CommitType>
    {
        public IEnumerable<(CommitType left, CommitType right)> GetEqualTestCases()
        {
            yield return (new CommitType("type"), new CommitType("type"));
            yield return (new CommitType("type"), new CommitType("TYPE"));
            yield return (CommitType.Feature, new CommitType("feat"));
            yield return (CommitType.BugFix, new CommitType("fix"));
        }

        public IEnumerable<(CommitType left, CommitType right)> GetUnequalTestCases()
        {
            yield return (new CommitType("type1"), new CommitType("type2"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("\t")]
        public void Type_must_not_be_null_or_whitespace(string type)
        {
            Assert.Throws<ArgumentException>(() => new CommitType(type));
        }

        [Theory]
        [InlineData("committype")]
        [InlineData("someOtherType")]
        public void ToString_returns_the_commit_type_as_string(string type)
        {
            var sut = new CommitType(type);
            Assert.Equal(type, sut.ToString());
        }


    }
}
