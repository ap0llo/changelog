using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Grynwald.ChangeLog.ConventionalCommits
{
    /// <summary>
    /// Base class for all parsers
    /// </summary>
    public abstract class Parser<TToken, TTokenKind>
        where TToken : Token<TTokenKind>
        where TTokenKind : Enum
    {
        private int m_Position = 0;
        private IReadOnlyList<TToken> m_Tokens = Array.Empty<TToken>();


        /// <summary>
        /// Gets the current token
        /// </summary>
        protected TToken Current => Peek(0);


        /// <summary>
        /// Gets a following token.
        /// </summary>
        /// <param name="lookAhead">The number of tokens from the current to look ahead. Value pf <c>0</c> gets <see cref="Current"/>.</param>
        /// <returns></returns>
        protected TToken Peek(int lookAhead)
        {
            return m_Position + lookAhead >= m_Tokens.Count
                ? m_Tokens[m_Tokens.Count - 1]
                : m_Tokens[m_Position + lookAhead];
        }

        /// <summary>
        /// Checks if the current token has the specified token kind and returns the token.
        /// Throws <see cref="UnexpectedTokenException"/> if current token doesn't match.
        /// </summary>
        protected TToken MatchToken(TTokenKind kind)
        {
            if (!Current.Kind.Equals(kind))
                throw new UnexpectedTokenException<TToken, TTokenKind>(kind, Current);

            var matchedToken = Current;
            m_Position += 1;
            return matchedToken;
        }

        /// <summary>
        /// Checks whether the current token is of the specified kind.
        /// </summary>
        /// <param name="kind">The kind to test the token for.</param>
        /// <returns></returns>
        protected bool TestToken(TTokenKind kind) => TestToken(kind, 0);

        /// <summary>
        /// Checks whether a token is of the specified kind.
        /// </summary>
        /// <param name="kind">The kind to test the token for.</param>
        /// <param name="lookAhead">The number of tokens from the current to look ahead. Value pf <c>0</c> tests <see cref="Current"/>.</param>
        /// <returns></returns>
        protected bool TestToken(TTokenKind kind, int lookAhead = 0) => Peek(lookAhead).Kind.Equals(kind);

        /// <summary>
        /// Checks whether a token is of the specified kind and has the specified value
        /// </summary>
        /// <param name="kind">The kind to test the token for.</param>
        /// <param name="value">The value to compare the token value to.</param>
        /// <param name="lookAhead">The number of tokens from the current to look ahead. Value pf <c>0</c> tests <see cref="Current"/>.</param>
        /// <returns></returns>
        protected bool TestToken(TTokenKind kind, string value, int offset = 0)
        {
            var token = Peek(offset);
            return token.Kind.Equals(kind) && token.Value == value;
        }

        /// <summary>
        /// Checks if the current token is of the specified kind and matches it.
        /// Operation is equivalent to calling <see cref="TestToken(TTokenKind)"/> and <see cref="MatchToken(TTokenKind)"/>.
        /// </summary>
        /// <param name="kind"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        protected bool TestAndMatchToken(TTokenKind kind, [NotNullWhen(true)]out TToken? token)
        {
            if (TestToken(kind))
            {
                token = MatchToken(kind);
                return true;
            }
            else
            {
                token = default;
                return false;
            }
        }

        /// <summary>
        /// Resets the parser to it's initial state.
        /// </summary>
        protected void Reset()
        {
            m_Tokens = GetTokens().ToArray();
            m_Position = 0;
        }


        protected abstract IReadOnlyList<TToken> GetTokens();
    }
}
