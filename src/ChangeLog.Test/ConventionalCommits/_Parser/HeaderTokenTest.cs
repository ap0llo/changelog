using System;
using Grynwald.ChangeLog.ConventionalCommits;
using Xunit;

namespace Grynwald.ChangeLog.Test.ConventionalCommits
{
    /// <summary>
    /// Tests for <see cref="HeaderToken"/>
    /// </summary>
    public class HeaderTokenTest
    {
        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public void Line_number_must_not_0_or_negative(int lineNumber)
        {
            Assert.Throws<ArgumentException>(() => new HeaderToken(HeaderTokenKind.String, "Some Value", lineNumber, 2));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public void Column_number_must_not_0_or_negative(int columnNumber)
        {
            Assert.Throws<ArgumentException>(() => new HeaderToken(HeaderTokenKind.String, "Some Value", 1, columnNumber));
        }

        [Theory]
        [CombinatorialData]
        public void Two_token_instances_are_equal_if_all_properties_are_equal(
            HeaderTokenKind kind,
            [CombinatorialValues(null, "", "\t", "some value")] string? value,
            [CombinatorialValues(1, 5)] int lineNumber,
            [CombinatorialValues(1, 5)] int columnNumber)
        {
            var token1 = new HeaderToken(kind, value, lineNumber, columnNumber);
            var token2 = new HeaderToken(kind, value, lineNumber, columnNumber);

            Assert.Equal(token1.GetHashCode(), token2.GetHashCode());
            Assert.Equal(token1, token2);
            Assert.Equal(token2, token1);
            Assert.True(token1.Equals(token2));
            Assert.True(token1.Equals((object)token2));
            Assert.True(token2.Equals(token1));
            Assert.True(token2.Equals((object)token1));
        }

        [Fact]
        public void Equals_returns_false_if_the_argument_if_not_a_HeaderToken()
        {
            var sut = new HeaderToken(HeaderTokenKind.String, "Some Value", 1, 2);
            Assert.False(sut.Equals(new object()));
        }
    }
}
