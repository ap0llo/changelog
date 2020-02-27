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
    
    /// <summary>
    /// Tokenizer that splits the input into a sequence of <see cref="FooterToken"/> values to be parsed by <see cref="FooterParser"/>
    /// </summary>
    public class FooterTokenizer : TokenizerBase<FooterToken>
    {
        protected override FooterToken CreateEolToken(int lineNumber, int columnNumber) =>
            FooterToken.Eol(lineNumber, columnNumber);

        protected override FooterToken CreateStringToken(string value, int lineNumber, int columnNumber) =>
            FooterToken.String(value, lineNumber, columnNumber);

        protected override bool TryMatchSingleCharToken(char value, int lineNumber, int columnNumber, [NotNullWhen(true)] out FooterToken? token)
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
