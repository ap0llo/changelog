using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grynwald.ChangeLog.ConventionalCommits
{
    /// <summary>
    /// Parser for the commit message header/subject.
    /// </summary>
    /// <seealso href="https://www.conventionalcommits.org">Conventional Commits</seealso>
    public sealed class HeaderParser : Parser<HeaderToken, HeaderTokenKind>
    {
        private readonly LineToken m_Input;
        private readonly HeaderTokenizer m_Tokenizer = new HeaderTokenizer();

        private HeaderParser(LineToken input)
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));

            if (input.Kind != LineTokenKind.Line)
                throw new ArgumentException($"Input must be line token of kind '{LineTokenKind.Line}', but is kind '{input.Kind}'");

            m_Input = input;
        }


        protected override IReadOnlyList<HeaderToken> GetTokens() => m_Tokenizer.GetTokens(m_Input).ToArray();

        private CommitMessageHeader Parse()
        {
            // Reset parser
            Reset();

            // parse type
            var type = new CommitType(MatchToken(HeaderTokenKind.String).Value!);

            // parse (optional) scope
            string? scope = null;
            if (TestAndMatchToken(HeaderTokenKind.OpenParenthesis, out _))
            {
                scope = MatchToken(HeaderTokenKind.String).Value;
                MatchToken(HeaderTokenKind.CloseParenthesis);
            }

            // parse "breaking change" (a optional '!' after the scope)
            var isBreakingChange = TestAndMatchToken(HeaderTokenKind.ExclamationMark, out _);

            // type and scope must be followed by ': '
            MatchToken(HeaderTokenKind.Colon);
            MatchToken(HeaderTokenKind.Space);

            // remaining tokens are the description
            var descriptionStartToken = Current;
            var desciptionBuilder = new StringBuilder();
            while (!TestToken(HeaderTokenKind.Eol))
            {
                desciptionBuilder.Append(MatchToken(Current.Kind).Value);
            }
            var description = desciptionBuilder.ToString();

            // description must not be empty
            if (String.IsNullOrWhiteSpace(description))
                throw new ParserException(descriptionStartToken, "Description must not be empty");

            // ensure the entire line was parsed
            MatchToken(HeaderTokenKind.Eol);

            return new CommitMessageHeader(type: type, description: description, scope: scope, isBreakingChange: isBreakingChange);
        }


        public static CommitMessageHeader Parse(LineToken input)
        {
            var parser = new HeaderParser(input);
            return parser.Parse();
        }
    }
}
