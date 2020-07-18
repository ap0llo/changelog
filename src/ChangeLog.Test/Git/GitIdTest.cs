using System;
using System.Collections.Generic;
using Grynwald.ChangeLog.Git;
using Xunit;

namespace Grynwald.ChangeLog.Test.Git
{
    public class GitIdTest : EqualityTest<GitId, GitIdTest>, IEqualityTestDataProvider<GitId>
    {
        public IEnumerable<(GitId left, GitId right)> GetEqualTestCases()
        {
            yield return (new GitId("8BADF00D"), new GitId("8BADF00D"));
            yield return (new GitId("8badF00d"), new GitId("8BADF00D"));

        }

        public IEnumerable<(GitId left, GitId right)> GetUnequalTestCases()
        {
            yield return (new GitId("abc123"), new GitId("def456"));
        }


        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        [InlineData("not-a-commit-id")]
        [InlineData("8BADF00D ")]
        [InlineData("  8BADF00D")]
        public void Constructor_throws_ArgumentException_if_input_is_not_a_valid_git_object_id(string id)
        {
            var ex = Assert.Throws<ArgumentException>(() => new GitId(id));
            Assert.Equal("id", ex.ParamName);
        }
    }
}
