using System;
using System.Collections;
using System.Collections.Generic;

namespace ChangeLogCreator.ConventionalCommits
{
    public abstract class Token : IEquatable<Token>
    {
        public string? Value { get; }

        public int LineNumber { get; }

        public int ColumnNumber { get; }


        protected Token(string? value, int lineNumber, int columnNumber)
        {
            if (lineNumber <= 0)
                throw new ArgumentException("Value must be greater or equal to 1", nameof(lineNumber));

            if (columnNumber <= 0)
                throw new ArgumentException("Value must be greater or equal to 1", nameof(columnNumber));

            Value = value;
            LineNumber = lineNumber;
            ColumnNumber = columnNumber;
        }


        public override int GetHashCode() => Value == null ? 0 : StringComparer.Ordinal.GetHashCode(Value);

        public override bool Equals(object? obj) => Equals(obj as Token);

        public bool Equals(Token? other) =>
            other != null &&
            StringComparer.Ordinal.Equals(Value, other.Value) &&
            LineNumber == other.LineNumber;

        public override string ToString() => $"({LineNumber}:{ColumnNumber}, '{Value}')";

    }

    public abstract class Token<TTokenKind> : Token, IEquatable<Token<TTokenKind>> where TTokenKind : Enum
    {
        public TTokenKind Kind { get; }

        protected Token(TTokenKind kind, string? value, int lineNumber, int columnNumber)
            : base(value, lineNumber, columnNumber)
        {
            Kind = kind;
        }

        
        public override bool Equals(object? obj) => Equals(obj as Token<TTokenKind>);

        public bool Equals(Token<TTokenKind>? other) =>
            other != null &&
            Equals(other as Token) &&
            Kind.Equals(other.Kind);

        public override string ToString() => $"({LineNumber}:{ColumnNumber}, {Kind}, '{Value}')";


        public void Deconstruct(out TTokenKind kind, out string? value)
        {
            kind = Kind;
            value = Value;
        }
    }

    internal abstract class Tokenizer<TToken, TTokenKind> : IEnumerable<TToken>
        where TToken : Token<TTokenKind>
        where TTokenKind : Enum
    {
        public abstract IEnumerator<TToken> GetEnumerator();
        
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
