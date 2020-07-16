using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grynwald.ChangeLog.ConventionalCommits
{
    /// <summary>
    /// Parser for commit messages following the <see href="https://www.conventionalcommits.org">Conventional Commits </see> format.
    /// </summary>
    /// <seealso href="https://www.conventionalcommits.org">Conventional Commits</see>
    /// <remarks>
    /// As different parsing rules apply to the different parts of the commit message (header/subject, body and footers),
    /// The parser and tokenizer is split into multiple parts.
    /// <para>
    /// This parser uses <see cref="LineTokenizer"/> that splits the input into either lines with content or "blank" lines.
    /// These lines are then parsed by the "second layer" of parsers (<see cref="HeaderParser"/> and <see cref="FooterParser"/>) that
    /// parse a single line and have their own tokenizers.
    /// </para>
    /// <para>
    /// The message body is handled directly in <see cref="CommitMessageParser"/> because the body has no inner structure and can be parsed by simply
    /// distinguishing between non-blank and blank lines.
    /// </para>
    /// </remarks>
    /// <seealso cref="HeaderParser"/>
    /// <seealso cref="FooterParser"/>
    public class CommitMessageParser : Parser<LineToken, LineTokenKind>
    {
        [Flags]
        private enum ParserMode
        {
            /// <summary>
            /// Use strict parsing
            /// </summary>
            Strict = 0x0,

            /// <summary>
            /// Use loose parsing (enable individual features listed below)
            /// </summary>
            Loose = IgnoreTrailingBlankLines | IgnoreDuplicateBlankLines | AllowBlankLinesBetweenFooters | TreatWhitespaceOnlyLinesAsBlankLines,

            /// <summary>
            /// Ignore trailing blank lines at the end of the commit message
            /// </summary>
            IgnoreTrailingBlankLines = 0x01,

            /// <summary>
            /// Treat all consecutive blank lines as a single blank line (e.g. allow multiple blank lines between header and message body)
            /// </summary>
            IgnoreDuplicateBlankLines = 0x01 << 1,

            /// <summary>
            /// Allow blank lines between footers
            /// </summary>
            AllowBlankLinesBetweenFooters = 0x01 << 2,

            /// <summary>
            /// Treat all lines that consist only of whitespace characters as blank lines
            /// </summary>
            TreatWhitespaceOnlyLinesAsBlankLines = 0x01 << 3
        }

        private readonly string m_Input;
        private readonly ParserMode m_Mode;


        private CommitMessageParser(string commitMessage, bool strictMode)
        {
            m_Input = commitMessage ?? throw new ArgumentNullException(nameof(commitMessage));
            m_Mode = strictMode ? ParserMode.Strict : ParserMode.Loose;
        }


        protected override IReadOnlyList<LineToken> GetTokens() => LineTokenizer.GetTokens(m_Input, m_Mode.HasFlag(ParserMode.TreatWhitespaceOnlyLinesAsBlankLines)).ToArray();

        private CommitMessage Parse()
        {
            // Reset parser
            Reset();

            // Parse Header
            var header = HeaderParser.Parse(MatchToken(LineTokenKind.Line));

            // Parse Body
            IReadOnlyList<string> body = Array.Empty<string>();
            if (!TestToken(LineTokenKind.Eof))
            {
                body = ParseBody();
            }

            // Parse Footer(s)
            IReadOnlyList<CommitMessageFooter> footers = Array.Empty<CommitMessageFooter>();
            if (!TestToken(LineTokenKind.Eof))
            {
                footers = ParseFooters();
            }

            // Consume all trailing blank lines (ignored unless in strict mode)
            if (m_Mode.HasFlag(ParserMode.IgnoreTrailingBlankLines))
            {
                MatchTokens(LineTokenKind.Blank, 0);
            }

            // a single trailing blank line is always allowed (message typically ends with line break)
            TestAndMatchToken(LineTokenKind.Blank, out _);

            // Ensure all tokens were parsed
            MatchToken(LineTokenKind.Eof);

            return new CommitMessage(header, body, footers);
        }

        private IReadOnlyList<string> ParseBody()
        {
            var body = new List<string>();

            // parse paragraphs until we reach the first footer
            while (TestForParagraph())
            {
                // Paragraphs are separated from earlier paragraphs and the subject line by a blank line
                // There must be at least one blank line,
                // multiple consecutive blank lines are treated the same as a single blank line if 'IgnoreDuplicateBlankLines ' is set
                if (m_Mode.HasFlag(ParserMode.IgnoreDuplicateBlankLines))
                {
                    MatchTokens(LineTokenKind.Blank, 1);
                }
                else
                {
                    MatchToken(LineTokenKind.Blank);
                }

                body.Add(ParseParagraph());
            }

            return body;
        }

        private string ParseParagraph()
        {
            var currentParagraph = new StringBuilder();

            foreach (var line in MatchTokens(LineTokenKind.Line, 0))
            {
                currentParagraph.AppendLine(line.Value);
            }

            return currentParagraph.ToString();
        }

        private bool TestForParagraph()
        {
            // both paragraphs and footer are separated from previous paragraphs and the header using a blank line.
            // If there is at least 1 blank line followed by a non-blank line,
            // the line could be either the first line of the next paragraph of the first footer.
            // To determine if we reached the end of the body, test if the next line is

            // When 'IgnoreDuplicateBlankLines' is set, skip all consecutive blank lines
            // otherwise, expect only a single blank line

            if (m_Mode.HasFlag(ParserMode.IgnoreDuplicateBlankLines))
            {
                return
                    TestTokens(LineTokenKind.Blank, out var blanklineCount) &&  // check for at least one blank line                                                                                
                    TestToken(LineTokenKind.Line, blanklineCount) &&            // check if blank lines are followed by a non-blank line(look ahead the number of blank lines matched)                                                                    
                    !FooterParser.IsFooter(Peek(blanklineCount));               // check if the line is the start of a footer
            }
            else
            {
                return
                    TestToken(LineTokenKind.Blank) &&
                    TestToken(LineTokenKind.Line, 1) &&
                    !FooterParser.IsFooter(Peek(1));
            }
        }

        private IReadOnlyList<CommitMessageFooter> ParseFooters()
        {
            // Footers are separated by the message body using a blank line.
            // There must be at least one blank line,
            // if the 'IgnoreDuplicateBlankLines' option is set,
            // multiple consecutive blank lines are treated the same as a single blank line.
            if (m_Mode.HasFlag(ParserMode.IgnoreDuplicateBlankLines))
            {
                MatchTokens(LineTokenKind.Blank, 1);
            }
            else
            {
                MatchToken(LineTokenKind.Blank);
            }

            var footers = new List<CommitMessageFooter>();


            // 'IgnoreTrailingBlankLines' is set, there might not be a parse-able footer present in the message
            // an we will only encounter blank lines, so it is not an error, if no non-blank line is found.
            // However if 'IgnoreTrailingBlankLines' is *not* set, there must be a at lease one footer after the blank line

            if (m_Mode.HasFlag(ParserMode.IgnoreTrailingBlankLines))
            {
                while (TestAndMatchToken(LineTokenKind.Line, out var currentLine))
                {
                    footers.Add(FooterParser.Parse(currentLine));

                    // ignore blank lines between footers when option is set
                    if (m_Mode.HasFlag(ParserMode.AllowBlankLinesBetweenFooters))
                    {
                        MatchTokens(LineTokenKind.Blank, 0);
                    }
                }
            }
            else if (!TestToken(LineTokenKind.Eof))
            {
                do
                {
                    var currentLine = MatchToken(LineTokenKind.Line);
                    footers.Add(FooterParser.Parse(currentLine));

                    // ignore blank lines between footers when option is set
                    if (m_Mode.HasFlag(ParserMode.AllowBlankLinesBetweenFooters))
                    {
                        MatchTokens(LineTokenKind.Blank, 0);
                    }

                } while (TestToken(LineTokenKind.Line));
            }

            return footers;
        }


        /// <summary>
        /// Parses the specified commit message.
        /// </summary>
        /// <param name="commitMessage">The commit message to be parsed.</param>
        /// <param name="strictMode">Use strict parsing settings.</param>
        /// <exception cref="ParserException">Thrown when the message could not be parsed</exception>
        public static CommitMessage Parse(string commitMessage, bool strictMode) => new CommitMessageParser(commitMessage, strictMode).Parse();
    }
}
