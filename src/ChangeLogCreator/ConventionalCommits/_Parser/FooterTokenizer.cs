using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace ChangeLogCreator.ConventionalCommits
{
    /// <summary>
    /// Enumerates the types of tokens emitted by <see cref="FooterTokenizer"/>
    /// </summary>
    public enum FooterTokenKind
    {
        String,     // any string value
        Colon,      // ':'
        Space,      // ' '
        Hash,       // '#'
        Eol         // end of input / last token
    }

    public sealed class FooterToken : Token<FooterTokenKind>
    {
        // Constructor should be private but internal for testing
        internal FooterToken(FooterTokenKind kind, string? value, int lineNumber, int columnNumber) : base(kind, value, lineNumber, columnNumber)
        { }


        public static FooterToken String(string value, int lineNumber, int columnNumber) =>
            new FooterToken(FooterTokenKind.String, value ?? throw new ArgumentNullException(nameof(value)), lineNumber, columnNumber);

        public static FooterToken Colon(int lineNumber, int columnNumber) =>
            new FooterToken(FooterTokenKind.Colon, ":", lineNumber, columnNumber);

        public static FooterToken Space(int lineNumber, int columnNumber) =>
            new FooterToken(FooterTokenKind.Space, " ", lineNumber, columnNumber);

        public static FooterToken Hash(int lineNumber, int columnNumber) =>
            new FooterToken(FooterTokenKind.Hash, "#", lineNumber, columnNumber);

        public static FooterToken Eol(int lineNumber, int columnNumber) =>
            new FooterToken(FooterTokenKind.Eol, null, lineNumber, columnNumber);
    }
    
    //TODO: Share code with HeaderTokenizer
    /// <summary>
    /// Tokenizer that splits the input into a sequence of <see cref="FooterToken"/> values to be parsed by <see cref="FooterParser"/>
    /// </summary>
    public static class FooterTokenizer
    {
        public static IEnumerable<FooterToken> GetTokens(LineToken input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            if (input.Kind != LineTokenKind.Line)
                throw new ArgumentException($"Input must be line token of kind '{LineTokenKind.Line}', but is kind '{input.Kind}'");


            var currentValue = new StringBuilder();

            var startColumn = 1;

            if (input.Value!.Length == 0)
            {
                yield return FooterToken.Eol(input.LineNumber, startColumn);
                yield break;
            }

            for (var i = 0; i < input.Value.Length; i++)
            {
                var currentChar = input.Value[i];
                if (TryMatchChar(currentChar, input.LineNumber, i + 1, out var matchedToken))
                {
                    if (currentValue.Length > 0)
                    {
                        yield return FooterToken.String(currentValue.GetValueAndClear(), input.LineNumber, startColumn);
                    }
                    yield return matchedToken;
                    startColumn = i + 2;
                }
                else
                {
                    currentValue.Append(currentChar);
                }

            }

            // if any input is left in currentValue, return it as String token
            if (currentValue.Length > 0)
            {
                yield return FooterToken.String(currentValue.GetValueAndClear(), input.LineNumber, startColumn);
            }

            yield return FooterToken.Eol(input.LineNumber, startColumn + 1);
        }

        private static bool TryMatchChar(char value, int lineNumber, int columnNumber, [NotNullWhen(true)] out FooterToken? token)
        {
            token = value switch
            {
                ':' => FooterToken.Colon(lineNumber, columnNumber),
                ' ' => FooterToken.Space(lineNumber, columnNumber),
                '#' => FooterToken.Hash(lineNumber, columnNumber),
                _ => null
            };

            return token != null;
        }
    }
}
