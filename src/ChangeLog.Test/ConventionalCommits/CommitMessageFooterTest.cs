using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using GitLabApiClient.Models.Releases.Responses;
using Grynwald.ChangeLog.ConventionalCommits;
using Xunit;

namespace Grynwald.ChangeLog.Test.ConventionalCommits
{
    /// <summary>
    /// Tests for <see cref="CommitMessageFooter"/>
    /// </summary>
    public class CommitMessageFooterTest
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("\t")]
        public void Value_must_not_be_null_or_whitespace(string value)
        {
            Assert.Throws<ArgumentException>(() => new CommitMessageFooter(new CommitMessageFooterName("name"), value));
        }


        [Fact]
        public void Two_CommitMessageFooter_instances_are_equal_if_both_Name_and_Value_are_equal()
        {
            var name = new CommitMessageFooterName("footer-name");
            var value = "Some Value";

            var instance1 = new CommitMessageFooter(name, value);
            var instance2 = new CommitMessageFooter(name, value);

            Assert.Equal(instance1.GetHashCode(), instance2.GetHashCode());
            Assert.Equal(instance1, instance2);
            Assert.True(instance1.Equals(instance2));
            Assert.True(instance1.Equals((object)instance2));
            Assert.True(instance2.Equals(instance1));
            Assert.True(instance2.Equals((object)instance1));
        }

        [Fact]
        public void Equals_retuns_false_if_argument_is_not_a_CommitMessageFooter()
        {
            var name = new CommitMessageFooterName("footer-name");
            var value = "Some Value";

            var sut = new CommitMessageFooter(name, value);
            Assert.False(sut.Equals(new object()));
        }
    }
}
