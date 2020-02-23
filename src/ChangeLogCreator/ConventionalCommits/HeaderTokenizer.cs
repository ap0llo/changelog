using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace ChangeLogCreator.ConventionalCommits
{
    internal enum HeaderTokenKind
    {
        String,             // any string value
        OpenParenthesis,    // '('
        CloseParenthesis,   // ')'
        Colon,              // ':'
        Space,              // ' '
        ExclamationMark,    // '!'
        Eol                 // end of input / last token
    }

    internal class HeaderToken : Token<HeaderTokenKind>
    {
        internal HeaderToken(HeaderTokenKind kind, string? value, int lineNumber, int columnNumber) : base(kind, value, lineNumber, columnNumber)
        { }

        public static HeaderToken String(string value, int lineNumber, int columnNumber) =>
            new HeaderToken(HeaderTokenKind.String, value, lineNumber, columnNumber);

        public static HeaderToken OpenParenthesis(int lineNumber, int columnNumber) =>
            new HeaderToken(HeaderTokenKind.OpenParenthesis, "(", lineNumber, columnNumber);

        public static HeaderToken CloseParenthesis(int lineNumber, int columnNumber) =>
            new HeaderToken(HeaderTokenKind.CloseParenthesis, ")", lineNumber, columnNumber);

        public static HeaderToken Colon(int lineNumber, int columnNumber) =>
            new HeaderToken(HeaderTokenKind.Colon, ":", lineNumber, columnNumber);

        public static HeaderToken Space(int lineNumber, int columnNumber) =>
            new HeaderToken(HeaderTokenKind.Space, " ", lineNumber, columnNumber);

        public static HeaderToken ExclamationMark(int lineNumber, int columnNumber) =>
            new HeaderToken(HeaderTokenKind.ExclamationMark, "!", lineNumber, columnNumber);

        public static HeaderToken Eol(int lineNumber, int columnNumber) =>
            new HeaderToken(HeaderTokenKind.Eol, null, lineNumber, columnNumber);
    }

    internal class HeaderTokenizer : Tokenizer<HeaderToken, HeaderTokenKind>
    {
        private readonly LineToken m_Input;

        public HeaderTokenizer(LineToken input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            if (input.Kind != LineTokenKind.Line)
                throw new ArgumentException($"Input must be line token of kind '{LineTokenKind.Line}', but is kind '{input.Kind}'");

            m_Input = input;
        }

        public override IEnumerator<HeaderToken> GetEnumerator()
        {
            var currentValue = new StringBuilder();

            int startColumn = 1;

            if(m_Input.Value!.Length == 0)
            {
                yield return HeaderToken.Eol(m_Input.LineNumber, startColumn);
                yield break;
            }

            for (var i = 0; i < m_Input.Value.Length; i++)
            {
                var currentChar = m_Input.Value[i];
                if(TryMatchChar(currentChar, i + 1, out var matchedToken))
                {
                    if (currentValue.Length > 0)
                    {
                        yield return HeaderToken.String(currentValue.GetValueAndClear(), m_Input.LineNumber, startColumn);
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
                yield return HeaderToken.String(currentValue.GetValueAndClear(), m_Input.LineNumber, startColumn);
            }

            yield return HeaderToken.Eol(m_Input.LineNumber, startColumn + 1);
        }
        
        private bool TryMatchChar(char value, int columnNumber, [NotNullWhen(true)] out HeaderToken? token)
        {
            token = value switch
            {
                '(' => HeaderToken.OpenParenthesis(m_Input.LineNumber, columnNumber),
                ')' => HeaderToken.CloseParenthesis(m_Input.LineNumber, columnNumber),
                ':' => HeaderToken.Colon(m_Input.LineNumber, columnNumber),
                ' ' => HeaderToken.Space(m_Input.LineNumber, columnNumber),
                '!' => HeaderToken.ExclamationMark(m_Input.LineNumber, columnNumber),
                _ => null
            };

            return token != null;            
        }


    }
}
