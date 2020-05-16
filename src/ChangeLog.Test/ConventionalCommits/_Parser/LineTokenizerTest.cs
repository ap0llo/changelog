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
            static object[] TestCase(string input, bool treatWhitespaceOnlyLinesAsBlankLines, params LineToken[] tokens)
            {
                return new object[]
                {
                    input,
                    treatWhitespaceOnlyLinesAsBlankLines,
                    tokens.Select(t => new XunitSerializableLineToken(t)).ToArray()
                };
            }

            // empty string
            yield return TestCase("", treatWhitespaceOnlyLinesAsBlankLines: false, LineToken.Eof(1));

            // 1 line without line break
            yield return TestCase("Line1", treatWhitespaceOnlyLinesAsBlankLines: false, LineToken.Line("Line1", 1), LineToken.Eof(2));

            foreach (var lineBreak in new[] { "\n", "\r\n" })
            {
                // just a line break
                yield return TestCase(lineBreak, treatWhitespaceOnlyLinesAsBlankLines: false, LineToken.Blank(1), LineToken.Eof(2));

                // 1 line with line break
                yield return TestCase("Line1" + lineBreak, treatWhitespaceOnlyLinesAsBlankLines: false, LineToken.Line("Line1", 1), LineToken.Eof(2));

                // 2 lines (with and without trailing line break)
                yield return TestCase(
                    "Line1" + lineBreak + "Line2" + lineBreak,
                    treatWhitespaceOnlyLinesAsBlankLines: false,
                    LineToken.Line("Line1", 1), LineToken.Line("Line2", 2), LineToken.Eof(3)
                );
                yield return TestCase(
                    "Line1" + lineBreak + "Line2",
                    treatWhitespaceOnlyLinesAsBlankLines: false,
                    LineToken.Line("Line1", 1), LineToken.Line("Line2", 2), LineToken.Eof(3)
                );

                // 2 lines with blank line in between (with and without trailing line break)
                yield return TestCase(
                    "Line1" + lineBreak + lineBreak + "Line2",
                    treatWhitespaceOnlyLinesAsBlankLines: false,
                    LineToken.Line("Line1", 1), LineToken.Blank(2), LineToken.Line("Line2", 3), LineToken.Eof(4)
                );
                yield return TestCase(
                    "Line1" + lineBreak + lineBreak + "Line2" + lineBreak,
                    treatWhitespaceOnlyLinesAsBlankLines: false,
                    LineToken.Line("Line1", 1), LineToken.Blank(2), LineToken.Line("Line2", 3), LineToken.Eof(4)
                );
            }

            // trailing blank line
            yield return TestCase(
                "feat: Some Description\r\n" +
                "\r\n" +
                "Message Body\r\n" +
                "\r\n" +
                "name: value\r\n" +
                "\r\n",
                treatWhitespaceOnlyLinesAsBlankLines: false,
                LineToken.Line("feat: Some Description", 1),
                LineToken.Blank(2),
                LineToken.Line("Message Body", 3),
                LineToken.Blank(4),
                LineToken.Line("name: value", 5),
                LineToken.Blank(6),
                LineToken.Eof(7)
            );

            // trailing blank line
            yield return TestCase(
                "feat: Some Description\r\n" +
                "\r\n",
                treatWhitespaceOnlyLinesAsBlankLines: false,
                LineToken.Line("feat: Some Description", 1),
                LineToken.Blank(2),
                LineToken.Eof(3)
            );

            // In "looseMode", whitespace-only lines are treated as empty lines
            // In strict mode, they are treated as regular lines
            foreach (var whitespaceLine in new[] { " ", "\t", "  " })
            {
                yield return TestCase(
                    "Line1\r\n" +
                    whitespaceLine + "\r\n" +
                    "Line3",
                    treatWhitespaceOnlyLinesAsBlankLines: true,
                    LineToken.Line("Line1", 1), LineToken.Blank(2), LineToken.Line("Line3", 3), LineToken.Eof(4)
                );


                yield return TestCase(
                    "Line1\r\n" +
                    whitespaceLine + "\r\n" +
                    "Line3",
                    treatWhitespaceOnlyLinesAsBlankLines: false,
                    LineToken.Line("Line1", 1), LineToken.Line(whitespaceLine, 2), LineToken.Line("Line3", 3), LineToken.Eof(4)
                );

                yield return TestCase(
                    "Line1\r\n" +
                    whitespaceLine + "\r\n" +
                    whitespaceLine + "\r\n" +
                    "Line4",
                    treatWhitespaceOnlyLinesAsBlankLines: true,
                    LineToken.Line("Line1", 1), LineToken.Blank(2), LineToken.Blank(3), LineToken.Line("Line4", 4), LineToken.Eof(5)
                );

                yield return TestCase(
                    "Line1\r\n" +
                    whitespaceLine + "\r\n" +
                    whitespaceLine + "\r\n" +
                    "Line4",
                    treatWhitespaceOnlyLinesAsBlankLines: false,
                    LineToken.Line("Line1", 1), LineToken.Line(whitespaceLine, 2), LineToken.Line(whitespaceLine, 3), LineToken.Line("Line4", 4), LineToken.Eof(5)
                );

                yield return TestCase(
                    "Line1\r\n" +
                    whitespaceLine + "\r\n" +
                    "Line3\r\n" +
                    whitespaceLine,
                    treatWhitespaceOnlyLinesAsBlankLines: true,
                    LineToken.Line("Line1", 1), LineToken.Blank(2), LineToken.Line("Line3", 3), LineToken.Blank(4), LineToken.Eof(5)
                );

                yield return TestCase(
                    "Line1\r\n" +
                    whitespaceLine + "\r\n" +
                    "Line3\r\n" +
                    whitespaceLine,
                    treatWhitespaceOnlyLinesAsBlankLines: false,
                    LineToken.Line("Line1", 1), LineToken.Line(whitespaceLine, 2), LineToken.Line("Line3", 3), LineToken.Line(whitespaceLine, 4), LineToken.Eof(5)
                );
            }

        }

        [Theory]
        [MemberData(nameof(TokenizerTestCases))]
        public void GetTokens_returns_expected_tokens(string input, bool treatWhitespaceOnlyLinesAsBlankLines, IEnumerable<XunitSerializableLineToken> expectedTokens)
        {
            // ARRANGE
            var inspectors = expectedTokens
                .Select(x => x.Value)
                .Select<LineToken, Action<LineToken>>(token => (t => Assert.Equal(token, t)))
                .ToArray();

            // ACT
            var actualTokens = LineTokenizer.GetTokens(input, treatWhitespaceOnlyLinesAsBlankLines).ToArray();

            // ASSERT
            Assert.Collection(actualTokens, inspectors);
        }
    }
}
