using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChangeLogCreator.ConventionalCommits
{
    internal class HeaderParser : Parser<HeaderToken, HeaderTokenKind>
    {
        private readonly LineToken m_Input;        

        private HeaderParser(LineToken input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            if (input.Kind != LineTokenKind.Line)
                throw new ArgumentException($"Input must be line token of kind '{LineTokenKind.Line}', but is kind '{input.Kind}'");

            m_Input = input;
        }


        public static CommitMessageHeader Parse(LineToken input)
        {
            var parser = new HeaderParser(input);
            return parser.Parse();
        }


        private CommitMessageHeader Parse()
        {
            Tokens = HeaderTokenizer.GetTokens(m_Input).ToArray();
            m_Position = 0;

            var parsed = new CommitMessageHeader();

            // parse type
            parsed.Type = MatchToken(HeaderTokenKind.String).Value!;

            // parse (optional) scope
            if(TestAndMatchToken(HeaderTokenKind.OpenParenthesis, out _))
            {
                parsed.Scope = MatchToken(HeaderTokenKind.String).Value;
                MatchToken(HeaderTokenKind.CloseParenthesis);
            }

            // parse "breaking change" (optional '!' after scope)
            parsed.IsBreakingChange = TestAndMatchToken(HeaderTokenKind.ExclamationMark, out _);

            // type and scope must be followed by ': '
            MatchToken(HeaderTokenKind.Colon);
            MatchToken(HeaderTokenKind.Space);

            // remaining tokens are the description
            
            var desciptionBuilder = new StringBuilder();
            while(!TestToken(HeaderTokenKind.Eol))
            {
                desciptionBuilder.Append(MatchToken(Current.Kind).Value);                
            }
            parsed.Description = desciptionBuilder.ToString();


            // description must not be empty
            if (String.IsNullOrWhiteSpace(parsed.Description))
            {
                throw new CommitMessageParserException("Description must not be empty");
            }

            // ensure the entire line was parsed
            MatchToken(HeaderTokenKind.Eol);

            return parsed;
        }


        

    }
}
