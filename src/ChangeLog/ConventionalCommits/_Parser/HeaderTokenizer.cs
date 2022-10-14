using System;
using System.Diagnostics.CodeAnalysis;

namespace Grynwald.ChangeLog.ConventionalCommits
{
    /// <summary>
    /// Enumerates the types of tokens emitted by <see cref="HeaderTokenizer"/>
    /// </summary>
    public enum HeaderTokenKind
    {
        String,             // any string value
        OpenParenthesis,    // '('
        CloseParenthesis,   // ')'
        Colon,              // ':'
        Space,              // ' '
        ExclamationMark,    // '!'
        Eol                 // end of input / last token
    }

    public sealed class HeaderToken : Token<HeaderTokenKind>
    {
        // Constructor should be private but internal for testing
        internal HeaderToken(HeaderTokenKind kind, string? value, int lineNumber, int columnNumber) : base(kind, value, lineNumber, columnNumber)
        { }


        public static HeaderToken String(string value, int lineNumber, int columnNumber) =>
            new HeaderToken(HeaderTokenKind.String, value ?? throw new ArgumentNullException(nameof(value)), lineNumber, columnNumber);

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

    /// <summary>
    /// Tokenizer that splits the input into a sequence of <see cref="HeaderToken"/> values to be parsed by <see cref="HeaderParser"/>
    /// </summary>
    public class HeaderTokenizer : TokenizerBase<HeaderToken>
    {
        protected override HeaderToken CreateEolToken(int lineNumber, int columnNumber) =>
            HeaderToken.Eol(lineNumber, columnNumber);

        protected override HeaderToken CreateStringToken(string value, int lineNumber, int columnNumber) =>
            HeaderToken.String(value, lineNumber, columnNumber);

        protected override bool TryMatchSingleCharToken(char value, int lineNumber, int columnNumber, [NotNullWhen(true)] out HeaderToken? token)
        {
            token = value switch
            {
                '(' => HeaderToken.OpenParenthesis(lineNumber, columnNumber),
                ')' => HeaderToken.CloseParenthesis(lineNumber, columnNumber),
                ':' => HeaderToken.Colon(lineNumber, columnNumber),
                ' ' => HeaderToken.Space(lineNumber, columnNumber),
                '!' => HeaderToken.ExclamationMark(lineNumber, columnNumber),
                _ => null
            };

            return token is not null;
        }
    }
}
