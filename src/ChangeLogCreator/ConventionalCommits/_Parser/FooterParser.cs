using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChangeLogCreator.ConventionalCommits
{    
    /// <summary>
    /// Parser for commit message footers.
    /// </summary>
    /// <seealso href="https://www.conventionalcommits.org">Conventional Commits</seealso>
    public sealed class FooterParser : Parser<FooterToken, FooterTokenKind>
    {
        private readonly LineToken m_Input;
        private readonly FooterTokenizer m_Tokenizer = new FooterTokenizer();


        private FooterParser(LineToken input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            if (input.Kind != LineTokenKind.Line)
                throw new ArgumentException($"Input must be line token of kind '{LineTokenKind.Line}', but is kind '{input.Kind}'");

            m_Input = input;
        }


        protected override IReadOnlyList<FooterToken> GetTokens() => m_Tokenizer.GetTokens(m_Input).ToArray();


        private CommitMessageFooter Parse()
        {
            // Reset parser
            Reset();

            // parse key
            var key = ParseKey();

            // remaining tokens are the description
            var descriptionStartToken = Current;
            var desciptionBuilder = new StringBuilder();
            while (!TestToken(FooterTokenKind.Eol))
            {
                desciptionBuilder.Append(MatchToken(Current.Kind).Value);
            }
            var footerDescription = desciptionBuilder.ToString();


            if (String.IsNullOrWhiteSpace(footerDescription))
                throw new ParserException(descriptionStartToken, "Footer description must not be empty");

            // make sure all input was parsed
            MatchToken(FooterTokenKind.Eol);

            return new CommitMessageFooter(key, footerDescription);
        }

        private bool IsFooterStart()
        {
            // Reset parser
            Reset();

            try
            {
                ParseKey();
                return true;
            }
            catch (ParserException)
            {
                return false;
            }

        }

        private CommitMessageFooterName ParseKey()
        {
            // Footers consist of a Key and a Description separated by either ": " or " #".
            // The footer key must not contain a space with one exception: "BREAKING CHANGE" is a valid key, too.
            // "BREAKING CHANGE" must be upper-case.
            //
            // So, footers must start with one of the following combinations of tokens:
            // - String     Colon Space
            // - String     Space Hash
            // - "BREAKING" Space "CHANGE" Colon Space
            // - "BREAKING" Space "CHANGE" Space Hash

            // the first token is always a string
            var typeToken = MatchToken(FooterTokenKind.String);

            if (TestAndMatchToken(FooterTokenKind.Space, out _))
            {
                // Remaining possibilities in this branch
                //  String      Space  Hash
                //  "BREAKING"  Space  "CHANGE" Colon Space
                //  "BREAKING"  Space  "CHANGE" Space Hash
                //     ^          ^      
                //     |          |      
                //     +----------+----------- already matched

                // "BREAKING CHANGE" is the only allowed footer type with a space in it
                // to handle that case, we need to look ahead to the next token
                if (typeToken.Value == "BREAKING" && TestToken(FooterTokenKind.String, "CHANGE"))
                {
                    MatchToken(FooterTokenKind.String);

                    // Remaining possibilities
                    // "BREAKING" Space "CHANGE"  Colon Space
                    // "BREAKING" Space "CHANGE"  Space Hash
                    //    ^         ^      ^
                    //    |         |      |
                    //    +---------+------+---- already matched
                    //

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

                    return CommitMessageFooterName.BreakingChange;
                }
                else
                {
                    // Remaining possibilities in this branch:
                    //   String Space Hash
                    //    ^       ^
                    //    |       |
                    //    +-------+-------- already matched
                    //
                    MatchToken(FooterTokenKind.Hash);

                    return new CommitMessageFooterName(typeToken.Value!);
                }
            }
            else
            {
                // Remaining possibilities in this branch:
                //  String Colon Space
                //    ^       
                //    |       
                //    +---------------- already matched
                //
                MatchToken(FooterTokenKind.Colon);
                MatchToken(FooterTokenKind.Space);

                return new CommitMessageFooterName(typeToken.Value!);
            }
        }


        /// <summary>
        /// Parses the specified commit footer.
        /// </summary>
        public static CommitMessageFooter Parse(LineToken input) => new FooterParser(input).Parse();

        /// <summary>
        /// Determines if the specified input is the start of a footer (i.e. the line starts with a valid footer key)
        /// </summary>
        public static bool IsFooter(LineToken input)
        {
            if (input.Kind != LineTokenKind.Line)
                return false;

            return new FooterParser(input).IsFooterStart();
        }
    }
}
