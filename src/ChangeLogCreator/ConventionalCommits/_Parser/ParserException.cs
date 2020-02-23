using System;
using System.Collections.Generic;
using System.Text;

namespace ChangeLogCreator.ConventionalCommits
{
    public class ParserException : Exception
    {
        public ParserException()
        {
        }
        //TODO: Include current token and position in input
        public ParserException(string message) : base(message)
        {

        }
    }

    internal class UnexpectedTokenException<TToken, TTokenKind> : ParserException
    {
        //TODO: Include current token and position in input
        public UnexpectedTokenException(TTokenKind expectedKind, TToken actualToken)
        {
        }
    }
}
