using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace ChangeLogCreator.ConventionalCommits
{
    enum FooterTokenKind
    {
        String,     // any string value
        Colon,      // ':'
        Space,      // ' '
        Hash,       // '#'
        Eol         // end of input / last token
    }

    internal class FooterToken : Token<FooterTokenKind>
    {
        private FooterToken(FooterTokenKind kind, string? value, int lineNumber, int columnNumber) : base(kind, value, lineNumber, columnNumber)
        { }


        public static FooterToken String(string value, int lineNumber, int columnNumber) =>
            new FooterToken(FooterTokenKind.String, value, lineNumber, columnNumber);

        public static FooterToken Colon(int lineNumber, int columnNumber) =>
            new FooterToken(FooterTokenKind.Colon, ":", lineNumber, columnNumber);

        public static FooterToken Space(int lineNumber, int columnNumber) =>
            new FooterToken(FooterTokenKind.Space, " ", lineNumber, columnNumber);

        public static FooterToken Hash(int lineNumber, int columnNumber) =>
            new FooterToken(FooterTokenKind.Hash, "#", lineNumber, columnNumber);

        public static FooterToken Eol(int lineNumber, int columnNumber) =>
            new FooterToken(FooterTokenKind.Eol, null, lineNumber, columnNumber);
    }

    //TODO: Tests
    //TODO: Share code with HeaderTokenizer
    class FooterTokenizer : Tokenizer<FooterToken, FooterTokenKind>
    {
        private readonly LineToken m_Input;

        public FooterTokenizer(LineToken input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            if (input.Kind != LineTokenKind.Line)
                throw new ArgumentException($"Input must be line token of kind '{LineTokenKind.Line}', but is kind '{input.Kind}'");

            m_Input = input;
        }

        public override IEnumerator<FooterToken> GetEnumerator()
        {
            var currentValue = new StringBuilder();

            int startColumn = 1;

            if (m_Input.Value!.Length == 0)
            {
                yield return FooterToken.Eol(m_Input.LineNumber, startColumn);
                yield break;
            }

            for (var i = 0; i < m_Input.Value.Length; i++)
            {
                var currentChar = m_Input.Value[i];
                if (TryMatchChar(currentChar, i + 1, out var matchedToken))
                {
                    if (currentValue.Length > 0)
                    {
                        yield return FooterToken.String(currentValue.GetValueAndClear(), m_Input.LineNumber, startColumn);
                    }
                    yield return matchedToken;
                    startColumn = i + 2;
                }
                else
                {
                    currentValue.Append(currentChar);
                }

            }

            // if any input is left in currentValueBuilder, return it as StringToken
            if (currentValue.Length > 0)
            {
                yield return FooterToken.String(currentValue.GetValueAndClear(), m_Input.LineNumber, startColumn);
            }

            yield return FooterToken.Eol(m_Input.LineNumber, startColumn + 1);
        }

        private bool TryMatchChar(char value, int columnNumber, [NotNullWhen(true)] out FooterToken? token)
        {
            token = value switch
            {
                ':' => FooterToken.Colon(m_Input.LineNumber, columnNumber),
                ' ' => FooterToken.Space(m_Input.LineNumber, columnNumber),
                '#' => FooterToken.Hash(m_Input.LineNumber, columnNumber),
                _ => null
            };

            return token != null;
        }
    }
}
