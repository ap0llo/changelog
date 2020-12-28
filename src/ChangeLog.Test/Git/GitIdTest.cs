using System;
using System.Collections.Generic;
using Grynwald.ChangeLog.Git;
using Xunit;

namespace Grynwald.ChangeLog.Test.Git
{
    /// <summary>
    /// Tests for <see cref="GitId"/>
    /// </summary>
    public class GitIdTest : EqualityTest<GitId, GitIdTest>, IEqualityTestDataProvider<GitId>
    {
        public IEnumerable<(GitId left, GitId right)> GetEqualTestCases()
        {
            yield return (new GitId("8BADF00D8BADF00D8BADF00D8BADF00D8BADF00D", "8BADF00"), new GitId("8BADF00D8BADF00D8BADF00D8BADF00D8BADF00D", "8BADF00"));
            yield return (new GitId("8badF00d8badF00d8badF00d8badF00d8badF00d", "8badf00"), new GitId("8BADF00D8BADF00D8BADF00D8BADF00D8BADF00D", "8BADF00"));

        }

        public IEnumerable<(GitId left, GitId right)> GetUnequalTestCases()
        {
            yield return (new GitId("abcd1234abcd1234abcd1234abcd1234abcd1234", "abcd123"), new GitId("efgh5678efgh5678efgh5678efgh5678efgh5678", "efgh567"));
        }


        [Theory]
        // Ids must not be null or whitespace
        [InlineData(null, "abc123")]
        [InlineData("", "abc123")]
        [InlineData(" ", "abc123")]
        [InlineData("\t", "abc123")]
        [InlineData("efgh5678efgh5678efgh5678efgh5678efgh5678", null)]
        [InlineData("efgh5678efgh5678efgh5678efgh5678efgh5678", "")]
        [InlineData("efgh5678efgh5678efgh5678efgh5678efgh5678", " ")]
        [InlineData("efgh5678efgh5678efgh5678efgh5678efgh5678", "\t")]
        // Ids must be a hex-string
        [InlineData("not-a-commit-id", "abc123")]
        [InlineData("abcd1234abcd1234abcd1234abcd1234abcd1234", "not-a-commit-id")]
        // Ids must not contain leading or trailing whitespace
        [InlineData("abcd1234abcd1234abcd1234abcd1234abcd1234 ", "abcd123")]
        [InlineData("  abcd1234abcd1234abcd1234abcd1234abcd1234", "abcd123")]
        [InlineData("abcd1234abcd1234abcd1234abcd1234abcd1234", "abcd123  ")]
        [InlineData("abcd1234abcd1234abcd1234abcd1234abcd1234", "  abcd123")]
        // Full id must be a full, 160bit/40-character git id
        [InlineData("8BADF00D", "8BADF00D")]
        [InlineData("8badf00d", "8BADF00D")]
        // Full id must start with abbreviated id
        [InlineData("abcd1234abcd1234abcd1234abcd1234abcd1234", "cde5678")]
        public void Constructor_throws_ArgumentException_if_input_is_not_a_valid_full_and_abbreviated_git_object_id(string id, string abbreviatedId)
        {
            // ARRANGE

            // ACT
            var ex = Record.Exception(() => new GitId(id, abbreviatedId));

            // ASSERT
            Assert.NotNull(ex);
            Assert.IsType<ArgumentException>(ex);
        }



        [Theory]
        [InlineData("abcd1234abcd1234abcd1234abcd1234abcd1234", "abcd123", true, "abcd123")]
        [InlineData("abcd1234abcd1234abcd1234abcd1234abcd1234", "abcd123", false, "abcd1234abcd1234abcd1234abcd1234abcd1234")]
        [InlineData("ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234", "ABCD123", true, "abcd123")]
        [InlineData("ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234", "ABCD123", false, "abcd1234abcd1234abcd1234abcd1234abcd1234")]
        public void ToString_returns_expected_value(string id, string abbreviatedId, bool abbreviate, string expected)
        {
            // ARRANGE
            var sut = new GitId(id, abbreviatedId);

            // ACT / ASSERT 
            Assert.Equal(id.ToLower(), sut.ToString());
            Assert.Equal(expected, sut.ToString(abbreviate));
        }

        [Fact]
        public void IsNull_returns_true_for_uninitialized_instance()
        {
            // ARRANGE
            var sut = default(GitId);

            // ACT 
            var isNull = sut.IsNull;

            // ASSERT
            Assert.True(isNull);
        }

        [Fact]
        public void IsNull_returns_false_for_initialized_instance()
        {
            // ARRANGE
            var sut = new GitId("abcd1234abcd1234abcd1234abcd1234abcd1234", "abcd123");

            // ACT 
            var isNull = sut.IsNull;

            // ASSERT
            Assert.False(isNull);
        }
    }
}
