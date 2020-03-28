using System;
using Grynwald.ChangeLog.Git;
using Xunit;

namespace Grynwald.ChangeLog.Test.Git
{
    public class GitIdTest
    {
        [Theory]
        [InlineData("8BADF00D", "8BADF00D")]
        [InlineData("8badF00d", "8BADF00D")]
        public void Two_GitId_instances_are_equal_when_the_id_shas_are_equal(string sha1, string sha2)
        {
            var id1 = new GitId(sha1);
            var id2 = new GitId(sha2);

            Assert.Equal(id1.GetHashCode(), id2.GetHashCode());
            Assert.Equal(id1, id2);
            Assert.True(id1 == id2);
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
