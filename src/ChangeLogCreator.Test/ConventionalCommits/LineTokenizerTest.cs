using System;
using System.Collections.Generic;
using System.Linq;
using ChangeLogCreator.ConventionalCommits;
using Xunit;

namespace ChangeLogCreator.Test.ConventionalCommits
{
    /// <summary>
    /// Tests for <see cref="LineTokenizer"/>
    /// </summary>
    public class LineTokenizerTest
    {
        public static IEnumerable<object[]> TokenizerTestCases()
        {
            object[] testCase(string input, params LineToken[] tokens) =>
                new object[]
                {
                    input,
                    tokens.Select(t => new XunitSerializableLineToken(t)).ToArray()
                };

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
        }

        [Theory]
        [MemberData(nameof(TokenizerTestCases))]
        public void Tokenizer_returns_expected_tokens(string input, IEnumerable<XunitSerializableLineToken> expectedTokens)
        {
            // ARRANGE
            var inspectors = expectedTokens
                .Select(x => x.Value)
                .Select<LineToken, Action<LineToken>>(token => (t => Assert.Equal(token, t)))
                .ToArray();

            var sut = new LineTokenizer(input);

            // ACT
            var actualTokens = sut.ToArray();

            // ASSERT
            Assert.Collection(actualTokens, inspectors);
        }
    }
}
