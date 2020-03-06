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

            yield return TestCase("key: value", new CommitMessageFooter(new CommitMessageFooterName("key"), "value"));
            yield return TestCase("key #value", new CommitMessageFooter(new CommitMessageFooterName("key"), "value"));
            yield return TestCase("key: value: with a colon", new CommitMessageFooter(new CommitMessageFooterName("key"), "value: with a colon"));
            yield return TestCase("key: value# with a hash", new CommitMessageFooter(new CommitMessageFooterName("key"), "value# with a hash"));
            yield return TestCase("breaking-change: change description", new CommitMessageFooter(new CommitMessageFooterName("breaking-change"), "change description"));
            yield return TestCase("breaking-change #change description", new CommitMessageFooter(new CommitMessageFooterName("breaking-change"), "change description"));
            yield return TestCase("BREAKING CHANGE: change description", new CommitMessageFooter(new CommitMessageFooterName("BREAKING CHANGE"), "change description"));
            yield return TestCase("BREAKING CHANGE #change description", new CommitMessageFooter(new CommitMessageFooterName("BREAKING CHANGE"), "change description"));
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
        [InlineData("T01", "", 1)]
        [InlineData("T02", "value", 6)]
        [InlineData("T03", "BREAKING Change: Description", 10)] // "BREAKING CHANGE" must be upper-case
        [InlineData("T04", "key:value", 5)]
        [InlineData("T05", "key : value", 5)]
        [InlineData("T06", "key#value", 4)]
        public void Parse_throws_CommitMessageParserException_for_invalid_input(string _, string input, int columnNumber)
        {
            // ARRANGE
            var lineNumber = 23;
            var inputToken = LineToken.Line(input, lineNumber);

            // ACT / ASSERT
            var ex = Assert.ThrowsAny<ParserException>(() => FooterParser.Parse(inputToken));
            Assert.Equal(lineNumber, ex.LineNumber);
            Assert.Equal(columnNumber, ex.ColumnNumber);
        }
    }
}
