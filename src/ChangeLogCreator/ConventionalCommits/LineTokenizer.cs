using System;
using System.Collections.Generic;
using System.Text;

namespace ChangeLogCreator.ConventionalCommits
{
    public enum LineTokenKind
    {
        Line,
        Blank,
        Eof
    }

    public sealed class LineToken : Token<LineTokenKind>
    {
        internal LineToken(LineTokenKind kind, string? value, int lineNumber) : base(kind, value, lineNumber, 1)
        { }

        public static LineToken Line(string value, int lineNumber)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            return new LineToken(LineTokenKind.Line, value, lineNumber);
        }

        public static LineToken Blank(int lineNumber) => new LineToken(LineTokenKind.Blank, "", lineNumber);

        public static LineToken Eof(int lineNumber) => new LineToken(LineTokenKind.Eof, null, lineNumber);
    }

    internal class LineTokenizer : Tokenizer<LineToken, LineTokenKind>
    {
        private readonly string m_Text;

        public LineTokenizer(string text)
        {
            m_Text = text ?? throw new ArgumentNullException(nameof(text));
        }

        public override IEnumerator<LineToken> GetEnumerator()
        {
            var currentLine = new StringBuilder();
            var lineNumber = 1;

            for (var i = 0; i < m_Text.Length; i++)
            {
                var currentChar = m_Text[i];
                var nextChar = i + 1 < m_Text.Length ? (char?)m_Text[i + 1] : null;

                switch ((currentChar, nextChar))
                {
                    case ('\r', '\n'):
                        if (currentLine.Length == 0)
                        {
                            yield return LineToken.Blank(lineNumber++);
                        }
                        else
                        {
                            yield return LineToken.Line(currentLine.GetValueAndClear(), lineNumber++);
                        }
                        i += 1;
                        break;

                    case ('\n', _):
                        if (currentLine.Length == 0)
                        {
                            yield return LineToken.Blank(lineNumber++);
                        }
                        else
                        {
                            yield return LineToken.Line(currentLine.GetValueAndClear(), lineNumber++);
                        }
                        break;

                    default:
                        currentLine.Append(currentChar);
                        break;
                }
            }

            if (currentLine.Length > 0)
            {
                yield return LineToken.Line(currentLine.GetValueAndClear(), lineNumber++);
            }

            yield return LineToken.Eof(lineNumber);
        }

    }
}
