using System;
using System.Collections.Generic;
using System.Linq;
using Grynwald.ChangeLog.ConventionalCommits;
using Xunit;

namespace Grynwald.ChangeLog.Test.ConventionalCommits
{
    /// <summary>
    /// Tests for <see cref="LineTokenizer"/>
    /// </summary>
    public class LineTokenizerTest
    {
        public static IEnumerable<object[]> TokenizerTestCases()
        {
            static object[] testCase(string input, params LineToken[] tokens)
            {
                return new object[]
                {
                    input,
                    tokens.Select(t => new XunitSerializableLineToken(t)).ToArray()
                };
            }

            // empty string
            yield return testCase("", LineToken.Eof(1));

            // 1 line without line break
            yield return testCase("Line1", LineToken.Line("Line1", 1), LineToken.Eof(2));

            foreach (var lineBreak in new[] { "\n", "\r\n" })
            {
                // just a line break
                yield return testCase(lineBreak, LineToken.Blank(1), LineToken.Eof(2));

                // 1 line with line break
                yield return testCase("Line1" + lineBreak, LineToken.Line("Line1", 1), LineToken.Eof(2));

                // 2 lines (with and without trailing line break)
                yield return testCase(
                    "Line1" + lineBreak + "Line2" + lineBreak,
                    LineToken.Line("Line1", 1), LineToken.Line("Line2", 2), LineToken.Eof(3)
                );
                yield return testCase(
                    "Line1" + lineBreak + "Line2",
                    LineToken.Line("Line1", 1), LineToken.Line("Line2", 2), LineToken.Eof(3)
                );

                // 2 lines with blank line in between (with and without trailing line break)
                yield return testCase(
                    "Line1" + lineBreak + lineBreak + "Line2",
                    LineToken.Line("Line1", 1), LineToken.Blank(2), LineToken.Line("Line2", 3), LineToken.Eof(4)
                );
                yield return testCase(
                    "Line1" + lineBreak + lineBreak + "Line2" + lineBreak,
                    LineToken.Line("Line1", 1), LineToken.Blank(2), LineToken.Line("Line2", 3), LineToken.Eof(4)
                );
            }

            // whitespace lines are treated as empty lines
            foreach (var whitespaceLine in new[] { " ", "\t", "  " })
            {
                yield return testCase(
                    "Line1\r\n" +
                    whitespaceLine + "\r\n" +
                    "Line3",
                    LineToken.Line("Line1", 1), LineToken.Blank(2), LineToken.Line("Line3", 3), LineToken.Eof(4)
                );

                yield return testCase(
                    "Line1\r\n" +
                    whitespaceLine + "\r\n" +
                    whitespaceLine + "\r\n" +
                    "Line4",
                    LineToken.Line("Line1", 1), LineToken.Blank(2), LineToken.Blank(3), LineToken.Line("Line4", 4), LineToken.Eof(5)
                );

                yield return testCase(
                    "Line1\r\n" +
                    whitespaceLine + "\r\n" +
                    "Line3\r\n" +
                    whitespaceLine,
                    LineToken.Line("Line1", 1), LineToken.Blank(2), LineToken.Line("Line3", 3), LineToken.Blank(4), LineToken.Eof(5)
                );
            }
        }

        [Theory]
        [MemberData(nameof(TokenizerTestCases))]
        public void GetTokens_returns_expected_tokens(string input, IEnumerable<XunitSerializableLineToken> expectedTokens)
        {
            // ARRANGE
            var inspectors = expectedTokens
                .Select(x => x.Value)
                .Select<LineToken, Action<LineToken>>(token => (t => Assert.Equal(token, t)))
                .ToArray();

            // ACT
            var actualTokens = LineTokenizer.GetTokens(input).ToArray();

            // ASSERT
            Assert.Collection(actualTokens, inspectors);
        }
    }
}
