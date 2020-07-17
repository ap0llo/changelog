using System;
using Grynwald.ChangeLog.Git;
using Xunit;

namespace Grynwald.ChangeLog.Test.Git
{
    /// <summary>
    /// Tests for <see cref="GitAuthor"/>
    /// </summary>
    public class GitAuthorTest
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("\t")]
        public void Name_must_not_be_null_or_whitespace(string name)
        {
            Assert.Throws<ArgumentException>(() => new GitAuthor(name, "user@example.com"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("\t")]
        public void Email_must_not_be_null_or_whitespace(string email)
        {
            Assert.Throws<ArgumentException>(() => new GitAuthor("user", email));
        }


        [Fact]
        public void Two_GitAuthor_instances_are_equal_if_both_Name_and_Email_are_equal()
        {
            var name = "user";
            var url = "user@example.com";

            var instance1 = new GitAuthor(name, url);
            var instance2 = new GitAuthor(name, url);

            Assert.Equal(instance1.GetHashCode(), instance2.GetHashCode());
            Assert.Equal(instance1, instance2);
            Assert.True(instance1.Equals(instance2));
            Assert.True(instance1.Equals((object)instance2));
            Assert.True(instance2.Equals(instance1));
            Assert.True(instance2.Equals((object)instance1));
        }

        [Fact]
        public void Equals_retuns_false_if_argument_is_not_a_GitAuthor()
        {
            var name = "user";
            var email = "user@example.com";

            var sut = new GitAuthor(name, email);
            Assert.False(sut.Equals(new object()));
        }
    }
}
