using System.Collections.Generic;
using ChangeLogCreator.ConventionalCommits;
using Xunit;

namespace ChangeLogCreator.Test.ConventionalCommits
{
    /// <summary>
    /// Tests for <see cref="HeaderParser"/>
    /// </summary>
    public class HeaderParserTest
    {
        public static IEnumerable<object[]> HeaderTestCases()
        {
            static object[] TestCase(string input, CommitMessageHeader expected)
            {
                return new object[] { input, new XunitSerializableCommitMessageHeader(expected) };
            }

            yield return TestCase("type: description", new CommitMessageHeader("type", "description"));
            yield return TestCase("type(scope): description", new CommitMessageHeader("type", "description", "scope"));
            yield return TestCase("type(scope)!: description", new CommitMessageHeader("type", "description", "scope", true));
            yield return TestCase("type!: description", new CommitMessageHeader("type", "description", null, true));
            yield return TestCase("type(scope)!: description with special character: () !", new CommitMessageHeader("type", "description with special character: () !", "scope", true));
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
        [InlineData("")]
        // Missing ': '
        [InlineData("feat")]
        [InlineData("feat Description")]
        [InlineData("feat(scope) Description")]
        // // Incomplete scope / missing ')' 
        [InlineData("type(scope: Description")]
        [InlineData("type scope): Description")]
        // missing description 
        [InlineData("feat:")]
        [InlineData("feat(scope):")]
        [InlineData("feat:\t")]
        [InlineData("feat(scope):\t")]
        [InlineData("feat: ")]
        [InlineData("feat(scope): ")]
        [InlineData("feat:  ")]
        [InlineData("feat(scope):  ")]
        public void Parse_throws_CommitMessageParserException_for_invalid_input(string input)
        {
            var inputToken = LineToken.Line(input, 1);
            Assert.ThrowsAny<ParserException>(() => HeaderParser.Parse(inputToken));

            //TODO: Check exception includes information about position where the error occurred
        }
    }
}
