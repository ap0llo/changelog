using System;
using System.Collections.Generic;
using System.Linq;
using ChangeLogCreator.ConventionalCommits;
using Xunit;

namespace ChangeLogCreator.Test.ConventionalCommits
{
    /// <summary>
    /// Tests for <see cref="HeaderTokenizer"/>
    /// </summary>
    public class HeaderTokenizerTest
    {
        public static IEnumerable<object[]> TokenizerTestCases()
        {
            static object[] TestCase(string input, params HeaderToken[] tokens) =>
                new object[]
                {
                    new XunitSerializableLineToken(LineToken.Line(input, 1)),
                    tokens.Select(t => new XunitSerializableHeaderToken(t)).ToArray()
                };

            yield return TestCase("", HeaderToken.Eol(1, 1));

            yield return TestCase(
                "type: Description text",
                HeaderToken.String("type", 1, 1),
                HeaderToken.Colon(1, 5),
                HeaderToken.Space(1, 6),
                HeaderToken.String("Description", 1, 7),
                HeaderToken.Space(1, 18),
                HeaderToken.String("text", 1, 19),
                HeaderToken.Eol(1, 20)
            );


            yield return TestCase(
               "type(scope): Description",
               HeaderToken.String("type", 1, 1),
               HeaderToken.OpenParenthesis(1, 5),
               HeaderToken.String("scope", 1, 6),
               HeaderToken.CloseParenthesis(1, 11),
               HeaderToken.Colon(1, 12),
               HeaderToken.Space(1, 13),
               HeaderToken.String("Description", 1, 14),
               HeaderToken.Eol(1, 26)
           );

            yield return TestCase(
               "type(scope)!: Description",
               HeaderToken.String("type", 1, 1),
               HeaderToken.OpenParenthesis(1, 5),
               HeaderToken.String("scope", 1, 6),
               HeaderToken.CloseParenthesis(1, 11),
               HeaderToken.ExclamationMark(1, 12),
               HeaderToken.Colon(1, 13),
               HeaderToken.Space(1, 14),
               HeaderToken.String("Description", 1, 15),
               HeaderToken.Eol(1, 27)
           );

            yield return TestCase(
               "type(scope): Description!",
               HeaderToken.String("type", 1, 1),
               HeaderToken.OpenParenthesis(1, 5),
               HeaderToken.String("scope", 1, 6),
               HeaderToken.CloseParenthesis(1, 11),
               HeaderToken.Colon(1, 12),
               HeaderToken.Space(1, 13),
               HeaderToken.String("Description", 1, 14),
               HeaderToken.ExclamationMark(1, 26),
               HeaderToken.Eol(1, 27)
            );


            yield return TestCase("feat:",   HeaderToken.String("feat", 1, 1), HeaderToken.Colon(1, 5), HeaderToken.Eol(1, 6) );
            yield return TestCase("feat: ",  HeaderToken.String("feat", 1, 1), HeaderToken.Colon(1, 5), HeaderToken.Space(1,6), HeaderToken.Eol(1, 7));
            yield return TestCase("feat:  ", HeaderToken.String("feat", 1, 1), HeaderToken.Colon(1, 5), HeaderToken.Space(1,6), HeaderToken.Space(1, 7), HeaderToken.Eol(1, 8));           
        }

        [Theory]
        [MemberData(nameof(TokenizerTestCases))]
        public void GetTokens_returns_expected_tokens(XunitSerializableLineToken input, IEnumerable<XunitSerializableHeaderToken> expectedTokens)
        {
            // ARRANGE
            var inspectors = expectedTokens
                .Select(x => x.Value)
                .Select<HeaderToken, Action<HeaderToken>>(token => (t => Assert.Equal(token, t)))
                .ToArray();
            
            // ACT
            var actualTokens = new HeaderTokenizer().GetTokens(input.Value).ToArray();

            // ASSERT
            Assert.Collection(actualTokens, inspectors);
        }

        [Fact]
        public void GetTokens_throws_ArgumentException_if_input_token_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new HeaderTokenizer().GetTokens(null!).ToArray());
        }

        [Fact]
        public void GetTokens_throws_ArgumentException_if_input_token_is_invalid()
        {
            Assert.Throws<ArgumentException>(() => new HeaderTokenizer().GetTokens(LineToken.Eof(1)).ToArray());
            Assert.Throws<ArgumentException>(() => new HeaderTokenizer().GetTokens(LineToken.Blank(1)).ToArray());
        }
    }
}
