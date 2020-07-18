using System;
using Grynwald.ChangeLog.ConventionalCommits;
using Xunit;

namespace Grynwald.ChangeLog.Test.ConventionalCommits
{
    /// <summary>
    /// Tests for <see cref="CommitType"/>
    /// </summary>
    public class CommitTypeTest
    {
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
        [InlineData("type", "type")]
        [InlineData("type", "TYPE")]
        public void Two_CommitType_instances_are_equal_if_the_type_is_the_same(string type1, string type2)
        {
            var instance1 = new CommitType(type1);
            var instance2 = new CommitType(type2);

            Assert.Equal(instance1.GetHashCode(), instance2.GetHashCode());
            Assert.Equal(instance1, instance2);
            Assert.True(instance1.Equals(instance2));
            Assert.True(instance1.Equals((object)instance2));
            Assert.True(instance2.Equals(instance1));
            Assert.True(instance2.Equals((object)instance1));
            Assert.True(instance1 == instance2);
            Assert.True(instance2 == instance1);
            Assert.False(instance1 != instance2);
            Assert.False(instance2 != instance1);
        }

        [Fact]
        public void Equals_retuns_false_if_argument_is_not_a_CommitType()
        {
            var sut = new CommitType("type");
            Assert.False(sut.Equals(new object()));
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
