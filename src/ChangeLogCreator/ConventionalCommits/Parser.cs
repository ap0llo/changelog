using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ChangeLogCreator.ConventionalCommits
{
    public abstract class Parser<TToken, TTokenKind>
        where TToken : Token<TTokenKind>
        where TTokenKind : Enum
    {
        protected int m_Position = 0;

        protected IReadOnlyList<TToken> Tokens { get; set; } = Array.Empty<TToken>();

        protected TToken Current => Peek(0);

        


        protected TToken MatchToken(TTokenKind kind)
        {
            if (!Current.Kind.Equals(kind))
            {
                throw new UnexpectedTokenException<TToken, TTokenKind>(kind, Current);
            }

            var matchedToken = Current;
            m_Position += 1;
            return matchedToken;
        }

        protected TToken Peek(int offset)
        {
            return m_Position + offset >= Tokens.Count ? Tokens[Tokens.Count - 1] : Tokens[m_Position + offset];
        }

        protected bool TestToken(TTokenKind kind, int offset = 0)
        {
            return Peek(offset).Kind.Equals(kind);            
        }

        protected bool TestToken(TTokenKind kind, string value, int offset = 0)
        {
            var token = Peek(offset);
            return token.Kind.Equals(kind) && token.Value == value;
        }

        protected bool TestAndMatchToken(TTokenKind kind, [NotNullWhen(true)]out TToken? token)
        {
            if(TestToken(kind))
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

    }
}
