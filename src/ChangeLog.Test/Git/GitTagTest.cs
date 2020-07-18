using System;
using System.Collections.Generic;
using Grynwald.ChangeLog.Git;
using Xunit;

namespace Grynwald.ChangeLog.Test.Git
{
    /// <summary>
    /// Tests for <see cref="GitTag"/>
    /// </summary>
    public class GitTagTest : EqualityTest<GitTag, GitTagTest>, IEqualityTestDataProvider<GitTag>
    {
        public IEnumerable<(GitTag left, GitTag right)> GetEqualTestCases()
        {
            yield return (new GitTag("tag1", new GitId("abc123")), new GitTag("tag1", new GitId("abc123")));
            yield return (new GitTag("tag1", new GitId("abc123")), new GitTag("tag1", new GitId("ABC123")));
        }

        public IEnumerable<(GitTag left, GitTag right)> GetUnequalTestCases()
        {
            yield return (new GitTag("tag1", new GitId("abc123")), new GitTag("tag2", new GitId("abc123")));
            yield return (new GitTag("tag1", new GitId("abc123")), new GitTag("tag1", new GitId("def456")));
        }


        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("\t")]
        public void Name_must_not_be_null_or_whitespace(string name)
        {
            Assert.Throws<ArgumentException>(() => new GitTag(name, new GitId("abc123")));
        }
    }
}
