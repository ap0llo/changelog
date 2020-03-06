using System.Collections.Generic;
using ChangeLogCreator.ConventionalCommits;
using Xunit;
using Xunit.Abstractions;

namespace ChangeLogCreator.Test.ConventionalCommits
{
    /// <summary>
    /// Tests for <see cref="HeaderParser"/>
    /// </summary>
    public class HeaderParserTest
    {
        private readonly ITestOutputHelper m_OutputHelper;

        public HeaderParserTest(ITestOutputHelper outputHelper) => m_OutputHelper = outputHelper;


        public static IEnumerable<object[]> HeaderTestCases()
        {
            static object[] TestCase(string input, CommitMessageHeader expected)
            {
                return new object[] { input, new XunitSerializableCommitMessageHeader(expected) };
            }

            yield return TestCase("type: description", new CommitMessageHeader(new CommitType("type"), "description"));
            yield return TestCase("type(scope): description", new CommitMessageHeader(new CommitType("type"), "description", "scope"));
            yield return TestCase("type(scope)!: description", new CommitMessageHeader(new CommitType("type"), "description", "scope", true));
            yield return TestCase("type!: description", new CommitMessageHeader(new CommitType("type"), "description", null, true));
            yield return TestCase("type(scope)!: description with special character: () !", new CommitMessageHeader(new CommitType("type"), "description with special character: () !", "scope", true));
        }

        [Theory]
        [MemberData(nameof(HeaderTestCases))]
        public void Parse_returns_expected_commit_message(string input, XunitSerializableCommitMessageHeader expected)
        {
            // ARRANGE
            var inputToken = LineToken.Line(input, 1);

            // ACT
            var parsed = HeaderParser.Parse(inputToken);

            // ASSERT
            Assert.Equal(expected.Value, parsed);
        }

        [Theory]
        [InlineData("T01", "", 1)]
        // Missing ': '
        [InlineData("T02", "feat", 5)]
        [InlineData("T03", "feat Description", 5)]
        [InlineData("T04", "feat(scope) Description", 12)]
        // // Incomplete scope / missing ')' 
        [InlineData("T05", "type(scope: Description", 11)]
        [InlineData("T06", "type scope): Description", 5)]
        // missing description 
        [InlineData("T07", "feat:", 6)]
        [InlineData("T08", "feat(scope):", 13)]
        [InlineData("T09", "feat:\t", 6)]
        [InlineData("T10", "feat(scope):\t", 13)]
        [InlineData("T11", "feat: ", 7)]
        [InlineData("T12", "feat(scope): ", 14)]
        [InlineData("T13", "feat:  ", 7)]
        [InlineData("T14", "feat(scope):  ", 14)]
        public void Parse_throws_CommitMessageParserException_for_invalid_input(string id, string input, int columnNumber)
        {
            m_OutputHelper.WriteLine($"Test case {id}");

            // ARRANGE
            var lineNumber = 5;
            var inputToken = LineToken.Line(input, lineNumber);

            // ACT / ASSERT
            var exception = Assert.ThrowsAny<ParserException>(() => HeaderParser.Parse(inputToken));
            Assert.Equal(lineNumber, exception.LineNumber);
            Assert.Equal(columnNumber, exception.ColumnNumber);            
        }
    }
}
