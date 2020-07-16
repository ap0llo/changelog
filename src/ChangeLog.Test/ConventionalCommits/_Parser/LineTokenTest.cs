using System;
using Grynwald.ChangeLog.ConventionalCommits;
using Xunit;

namespace Grynwald.ChangeLog.Test.ConventionalCommits
{
    /// <summary>
    /// Tests for <see cref="LineToken"/>
    /// </summary>
    public class LineTokenTest
    {
        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public void Line_number_must_not_0_or_negative(int lineNumber)
        {
            Assert.Throws<ArgumentException>(() => new LineToken(LineTokenKind.Line, "Some Value", lineNumber));
        }

        [Theory]
        [CombinatorialData]
        public void Two_token_instances_are_equal_if_all_properties_are_equal(
            LineTokenKind kind,
            [CombinatorialValues(null, "", "\t", "some value")] string? value,
            [CombinatorialValues(1, 5)] int lineNumber)
        {
            var token1 = new LineToken(kind, value, lineNumber);
            var token2 = new LineToken(kind, value, lineNumber);

            Assert.Equal(token1.GetHashCode(), token2.GetHashCode());
            Assert.Equal(token1, token2);
            Assert.Equal(token2, token1);
            Assert.True(token1.Equals(token2));
            Assert.True(token1.Equals((object)token2));
            Assert.True(token2.Equals(token1));
            Assert.True(token2.Equals((object)token1));
        }

        [Fact]
        public void Equals_returns_false_if_the_argument_if_not_a_LineToken()
        {
            var sut = new LineToken(LineTokenKind.Line, "Some Value", 1);
            Assert.False(sut.Equals(new object()));
        }
    }
}
