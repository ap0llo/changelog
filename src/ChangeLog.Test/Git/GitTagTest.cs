using System;
using Grynwald.ChangeLog.Git;
using Xunit;

namespace Grynwald.ChangeLog.Test.Git
{
    /// <summary>
    /// Tests for <see cref="GitTag"/>
    /// </summary>
    public class GitTagTest
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("\t")]
        public void Name_must_not_be_null_or_whitespace(string name)
        {
            Assert.Throws<ArgumentException>(() => new GitTag(name, new GitId("abc123")));
        }


        [Fact]
        public void Two_GitTag_instances_are_equal_if_both_Name_and_Commit_are_equal()
        {
            var name = "origin";
            var commit = new GitId("abc123");

            var instance1 = new GitTag(name, commit);
            var instance2 = new GitTag(name, commit);

            Assert.Equal(instance1.GetHashCode(), instance2.GetHashCode());
            Assert.Equal(instance1, instance2);
            Assert.True(instance1.Equals(instance2));
            Assert.True(instance1.Equals((object)instance2));
            Assert.True(instance2.Equals(instance1));
            Assert.True(instance2.Equals((object)instance1));
        }

        [Fact]
        public void Equals_retuns_false_if_argument_is_not_a_GitRemote()
        {
            var name = "origin";
            var commit = new GitId("abc123");

            var sut = new GitTag(name, commit);
            Assert.False(sut.Equals(new object()));
        }
    }
}
