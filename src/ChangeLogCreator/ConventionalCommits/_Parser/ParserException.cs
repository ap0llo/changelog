using System;

namespace ChangeLogCreator.ConventionalCommits
{
    public class ParserException : Exception
    {
        /// <summary>
        /// Gets the line number in the input where the parser error occurred
        /// </summary>
        public int LineNumber { get; }

        /// <summary>
        /// Gets the column number in the input where the parser error occurred
        /// </summary>
        public int ColumnNumber { get; }


        public ParserException(Token currentToken, string message) : base(message)
        {
            LineNumber = currentToken.LineNumber;
            ColumnNumber = currentToken.ColumnNumber;
        }
    }

    public class UnexpectedTokenException<TToken, TTokenKind> : ParserException where TToken : Token
    {
        public UnexpectedTokenException(TTokenKind expectedKind, TToken actualToken) : base(actualToken, $"Unexpected token '{actualToken}', expected '{expectedKind}' token")
        { }
    }
}
