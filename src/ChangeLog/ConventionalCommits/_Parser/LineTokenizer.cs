﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Grynwald.ChangeLog.ConventionalCommits
{
    /// <summary>
    /// Enumerates the types of tokens emitted by <see cref="LineTokenizer"/>
    /// </summary>
    public enum LineTokenKind
    {
        Line,
        Blank,
        Eof
    }

    public sealed class LineToken : Token<LineTokenKind>
    {
        // Constructor should be private but internal for testing
        internal LineToken(LineTokenKind kind, string? value, int lineNumber) : base(kind, value, lineNumber, 1)
        { }

        public static LineToken Line(string value, int lineNumber) =>
            new LineToken(LineTokenKind.Line, value ?? throw new ArgumentNullException(nameof(value)), lineNumber);

        public static LineToken Blank(int lineNumber) => new LineToken(LineTokenKind.Blank, "", lineNumber);

        public static LineToken Eof(int lineNumber) => new LineToken(LineTokenKind.Eof, null, lineNumber);
    }

    /// <summary>
    /// Tokenizer that splits the input into a series of "Blank Line" and "Line" (= non-blank) tokens. 
    /// </summary>
    internal static class LineTokenizer
    {
        public static IEnumerable<LineToken> GetTokens(string input, bool treatWhitespaceOnlyLinesAsBlankLines)
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));

            var currentLine = new StringBuilder();
            var currentLineIsWhitespace = true;
            var lineNumber = 1;

            LineToken GetCurrentToken()
            {
                // In "Loose Mode", lines that consist only of whitespace characters are considered blank lines                
                if (currentLine.Length == 0 || (treatWhitespaceOnlyLinesAsBlankLines && currentLineIsWhitespace))
                {
                    // clear current value of line
                    currentLine.Clear();
                    return LineToken.Blank(lineNumber++);
                }
                else
                {
                    // get current value and reset state
                    var value = currentLine.GetValueAndClear();
                    currentLineIsWhitespace = true;
                    return LineToken.Line(value, lineNumber++);
                }
            }

            for (var i = 0; i < input.Length; i++)
            {
                var currentChar = input[i];
                var nextChar = i + 1 < input.Length ? (char?)input[i + 1] : null;

                switch ((currentChar, nextChar))
                {
                    case ('\r', '\n'):
                        yield return GetCurrentToken();
                        i += 1;
                        break;

                    case ('\n', _):
                        yield return GetCurrentToken();
                        break;

                    default:
                        currentLineIsWhitespace &= char.IsWhiteSpace(currentChar);
                        currentLine.Append(currentChar);
                        break;
                }
            }

            if (currentLine.Length > 0)
            {
                yield return GetCurrentToken();
            }

            yield return LineToken.Eof(lineNumber);
        }
    }
}
