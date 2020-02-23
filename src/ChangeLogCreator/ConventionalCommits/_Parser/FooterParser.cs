using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChangeLogCreator.ConventionalCommits
{
    //TODO: Add tests
    class FooterParser : Parser<FooterToken, FooterTokenKind>
    {
        private readonly LineToken m_Input;

        private FooterParser(LineToken input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            if (input.Kind != LineTokenKind.Line)
                throw new ArgumentException($"Input must be line token of kind '{LineTokenKind.Line}', but is kind '{input.Kind}'");

            m_Input = input;
        }

    
        public static CommitMessageFooter Parse(LineToken input)
        {
            return new FooterParser(input).Parse();
        }


        public static bool IsFooter(LineToken input)
        {
            if (input.Kind != LineTokenKind.Line)
                return false;

            return new FooterParser(input).IsFooterStart();
        }

        private CommitMessageFooter Parse()
        {
            Tokens = FooterTokenizer.GetTokens(m_Input).ToArray();
            m_Position = 0;
            

            var footerType = ParseFooterType();

            // remaining tokens are the description
            var desciptionBuilder = new StringBuilder();
            while (!TestToken(FooterTokenKind.Eol))
            {
                desciptionBuilder.Append(MatchToken(Current.Kind).Value);
            }
            var footerDescription = desciptionBuilder.ToString();

            if (String.IsNullOrWhiteSpace(footerDescription))
            {
                throw new CommitMessageParserException("Footer description must not be empty");
            }

            MatchToken(FooterTokenKind.Eol);

            return new CommitMessageFooter(footerType, footerDescription);          
        }

        private bool IsFooterStart()
        {
            Tokens = FooterTokenizer.GetTokens(m_Input).ToArray();
            m_Position = 0;


            try
            {
                ParseFooterType();
                return true;
            }
            catch (CommitMessageParserException)
            {
                return false;
            }

        }

        private string ParseFooterType()
        {
            // footers start with either:
            // - String     Colon Space
            // - String     Space Hash
            // - "BREAKING" Space "CHANGE" Colon Space
            // - "BREAKING" Space "CHANGE" Space Hash

            var typeToken = MatchToken(FooterTokenKind.String);

            if (TestAndMatchToken(FooterTokenKind.Space, out _))
            {
                // Remaining possibilities
                // - (String      Space) Hash
                // - ("BREAKING"  Space) "CHANGE" Colon Space
                // - ("BREAKING"  Space) "CHANGE" Space Hash

                // "BREAKING CHANGE" is the only allowed footer type with a space in it
                // to handle that case, we need to look ahead to the next token
                if (typeToken.Value == "BREAKING" && TestToken(FooterTokenKind.String, "CHANGE"))                    
                {
                    // Remaining possibilities
                    // - ("BREAKING" Space "CHANGE") Colon Space
                    // - ("BREAKING" Space "CHANGE") Space Hash

                    MatchToken(FooterTokenKind.String);

                    if (TestToken(FooterTokenKind.Colon))
                    {
                        MatchToken(FooterTokenKind.Colon);
                        MatchToken(FooterTokenKind.Space);
                    }
                    else
                    {
                        MatchToken(FooterTokenKind.Space);
                        MatchToken(FooterTokenKind.Hash);
                    }

                    return "BREAKING CHANGE";
                }
                else
                {
                    // Remaining possibilities
                    // - (String Space) Hash
                    MatchToken(FooterTokenKind.Hash);

                    return typeToken.Value!;
                }
            }
            else
            {
                // Remaining possibilities
                // - (String) Colon Space
                MatchToken(FooterTokenKind.Colon);
                MatchToken(FooterTokenKind.Space);

                return typeToken.Value!;
            }

        }
    }
}
