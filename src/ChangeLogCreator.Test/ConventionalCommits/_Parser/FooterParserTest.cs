using System.Collections.Generic;
using ChangeLogCreator.ConventionalCommits;
using Xunit;

namespace ChangeLogCreator.Test.ConventionalCommits
{
    /// <summary>
    /// Tests for <see cref="FooterParser"/>
    /// </summary>
    public class FooterParserTest
    {
        public static IEnumerable<object[]> FooterTestCases()
        {
            static object[] TestCase(string input, CommitMessageFooter expected)
            {
                return new object[] { input, new XunitSerializableCommitMessageFooter(expected) };
            }

            yield return TestCase("key: value", new CommitMessageFooter("key", "value"));
            yield return TestCase("key #value", new CommitMessageFooter("key", "value"));
            yield return TestCase("key: value: with a colon", new CommitMessageFooter("key", "value: with a colon"));
            yield return TestCase("key: value# with a hash", new CommitMessageFooter("key", "value# with a hash"));
            yield return TestCase("breaking-change: change description", new CommitMessageFooter("breaking-change", "change description"));
            yield return TestCase("breaking-change #change description", new CommitMessageFooter("breaking-change", "change description"));
            yield return TestCase("BREAKING CHANGE: change description", new CommitMessageFooter("BREAKING CHANGE", "change description"));
            yield return TestCase("BREAKING CHANGE #change description", new CommitMessageFooter("BREAKING CHANGE", "change description"));
        }

        [Theory]
        [MemberData(nameof(FooterTestCases))]
        public void Parse_returns_expected_commit_message(string input, XunitSerializableCommitMessageFooter expected)
        {
            // ARRANGE
            var inputToken = LineToken.Line(input, 1);

            // ACT
            var parsed = FooterParser.Parse(inputToken);

            // ASSERT
            Assert.Equal(expected.Value, parsed);
        }

        [Theory]
        [InlineData("")]
        [InlineData("value")]
        [InlineData("BREAKING Change: Description")] // "BREAKING CHANGE" must be upper-case
        [InlineData("key:value")]
        [InlineData("key : value")]
        [InlineData("key#value")]
        public void Parse_throws_CommitMessageParserException_for_invalid_input(string input)
        {
            var inputToken = LineToken.Line(input, 1);
            Assert.ThrowsAny<ParserException>(() => FooterParser.Parse(inputToken));

            //TODO: Check exception includes information about position where the error occurred
        }
    }
}
