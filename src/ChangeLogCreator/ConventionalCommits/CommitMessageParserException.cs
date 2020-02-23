using System;
using System.Collections.Generic;
using System.Text;

namespace ChangeLogCreator.ConventionalCommits
{
    public class CommitMessageParserException : Exception
    {
        public CommitMessageParserException()
        {
        }

        public CommitMessageParserException(string message) : base(message)
        {

        }
    }

    internal class UnexpectedTokenException<TToken, TTokenKind> : CommitMessageParserException
    {
        public UnexpectedTokenException(TTokenKind expectedKind, TToken actualToken)
        {
        }
    }
}
