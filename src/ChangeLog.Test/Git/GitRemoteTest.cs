using System;
using Grynwald.ChangeLog.Git;
using Xunit;

namespace Grynwald.ChangeLog.Test.Git
{
    /// <summary>
    /// Tests for <see cref="GitRemote"/>
    /// </summary>
    public class GitRemoteTest
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("\t")]
        public void Name_must_not_be_null_or_whitespace(string name)
        {
            Assert.Throws<ArgumentException>(() => new GitRemote(name, "http://example.com"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("\t")]
        public void Url_must_not_be_null_or_whitespace(string url)
        {
            Assert.Throws<ArgumentException>(() => new GitRemote("origin", url));
        }


        [Fact]
        public void Two_GitRemote_instances_are_equal_if_both_Name_and_Url_are_equal()
        {
            var name = "origin";
            var url = "http://example.com";

            var instance1 = new GitRemote(name, url);
            var instance2 = new GitRemote(name, url);

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
            var url = "http://example.com";

            var sut = new GitRemote(name, url);
            Assert.False(sut.Equals(new object()));
        }
    }
}
