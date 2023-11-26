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
            yield return (new GitTag("tag1", TestGitIds.Id1), new GitTag("tag1", TestGitIds.Id1));
            yield return (new GitTag("tag1", TestGitIds.Id1), new GitTag("tag1", TestGitIds.Id1));
        }

        public IEnumerable<(GitTag left, GitTag right)> GetUnequalTestCases()
        {
            yield return (
                new GitTag("tag1", TestGitIds.Id1),
                new GitTag("tag2", TestGitIds.Id1)
            );

            yield return (
                new GitTag("tag1", TestGitIds.Id1),
                new GitTag("tag1", TestGitIds.Id2)
            );
        }


        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("\t")]
        public void Name_must_not_be_null_or_whitespace(string? name)
        {
            Assert.Throws<ArgumentException>(() => new GitTag(name!, TestGitIds.Id1));
        }

        [Fact]
        public void Commit_must_not_be_null()
        {
            // ARRANGE
            var commit = default(GitId);

            // ACT 
            var ex = Record.Exception(() => new GitTag("tageName", commit));

            // ASSERT
            Assert.NotNull(ex);
            var argumentException = Assert.IsType<ArgumentException>(ex);
            Assert.Equal("commit", argumentException.ParamName);
        }
    }
}
