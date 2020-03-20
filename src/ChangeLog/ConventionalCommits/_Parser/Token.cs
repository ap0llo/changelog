using System;

namespace Grynwald.ChangeLog.ConventionalCommits
{
    public abstract class Token : IEquatable<Token>
    {
        /// <summary>
        /// Gets the value of the token.
        /// </summary>
        public string? Value { get; }

        /// <summary>
        /// Gets the line number of the token in the input string.
        /// </summary>
        public int LineNumber { get; }

        /// <summary>
        /// Gets the column number of the token in the input string.
        /// </summary>
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


        /// <inheritdoc />
        public override int GetHashCode() => Value == null ? 0 : StringComparer.Ordinal.GetHashCode(Value);

        /// <inheritdoc />
        public override bool Equals(object? obj) => Equals(obj as Token);

        /// <inheritdoc />
        public bool Equals(Token? other) =>
            other != null &&
            StringComparer.Ordinal.Equals(Value, other.Value) &&
            LineNumber == other.LineNumber;

        /// <inheritdoc />
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


        /// <inheritdoc />
        public override int GetHashCode() => base.GetHashCode();

        /// <inheritdoc />
        public override bool Equals(object? obj) => Equals(obj as Token<TTokenKind>);

        /// <inheritdoc />
        public bool Equals(Token<TTokenKind>? other) =>
            other != null &&
            Equals(other as Token) &&
            Kind.Equals(other.Kind);

        /// <inheritdoc />
        public override string ToString() => $"({LineNumber}:{ColumnNumber}, {Kind}, '{Value}')";
    }
}
