using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChangeLogCreator.ConventionalCommits
{
    public class CommitMessageParser: Parser<LineToken, LineTokenKind>
    {
        private readonly string m_Input;
        
        private CommitMessageParser(string commitMessage)
        {
            m_Input = commitMessage ?? throw new ArgumentNullException(nameof(commitMessage));

        }


        public static CommitMessage Parse(string commitMessage)
        {
            var parser = new CommitMessageParser(commitMessage);
            return parser.Parse();

        }


        private CommitMessage Parse()
        {
            Tokens = LineTokenizer.GetTokens(m_Input).ToArray();
            m_Position = 0;

            
            // Parse Header
            //TODO: Add a separate type for the header, similar to footer
            var firstLine = MatchToken(LineTokenKind.Line);
            var header = HeaderParser.Parse(firstLine);

            
            // Parse Body
            IReadOnlyList<string> body = Array.Empty<string>();
            if(!TestToken(LineTokenKind.Eof))
            {
                body = ParseBody();
            }

            // Parse Footer(s)
            IReadOnlyList<CommitMessageFooter> footers = Array.Empty<CommitMessageFooter>();
            if (!TestToken(LineTokenKind.Eof))
            {
                MatchToken(LineTokenKind.Blank);
                footers = ParseFooters();
            }

            // Ensure all tokens were parsed
            MatchToken(LineTokenKind.Eof);

            return new CommitMessage(header, body, footers);
        }


        private IReadOnlyList<string> ParseBody()
        {
            var body = new List<string>();

            while (TestForParagraph())
            {
                MatchToken(LineTokenKind.Blank);
                body.Add(ParseParagraph());
            }

            return body;
        }

        private string ParseParagraph()
        {           
            var currentParagraph = new StringBuilder();

            while (TestAndMatchToken(LineTokenKind.Line, out var line))
            {
                currentParagraph.AppendLine(line.Value);
            }

            return currentParagraph.ToString();

                       
        }

        private bool TestForParagraph()
        {
            // if there is a blank after the paragraph, there might be another paragraph or one or more footers

            return
                TestToken(LineTokenKind.Blank) &&
                TestToken(LineTokenKind.Line, 1) &&
                !FooterParser.IsFooter(Peek(1));           
        }

        private IReadOnlyList<CommitMessageFooter> ParseFooters()
        {
            var footers = new List<CommitMessageFooter>();
            while (TestAndMatchToken(LineTokenKind.Line, out var currentLine))
            {
                footers.Add(FooterParser.Parse(currentLine));
            }
            return footers;
        }
    }
}
