using System;

namespace Grynwald.ChangeLog.ConventionalCommits
{
    public abstract class Token
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
        public abstract override int GetHashCode();

        /// <inheritdoc />
        public abstract override bool Equals(object? obj);

        /// <inheritdoc />
        public abstract override string ToString();

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
        public override int GetHashCode() => Value is null ? 0 : StringComparer.Ordinal.GetHashCode(Value);

        /// <inheritdoc />
        public override bool Equals(object? obj) => Equals(obj as Token<TTokenKind>);

        /// <inheritdoc />
        public bool Equals(Token<TTokenKind>? other) =>
            other is not null &&
            StringComparer.Ordinal.Equals(Value, other.Value) &&
            LineNumber == other.LineNumber &&
            ColumnNumber == other.ColumnNumber &&
            Kind.Equals(other.Kind);

        /// <inheritdoc />
        public override string ToString() => $"({LineNumber}:{ColumnNumber}, {Kind}, '{Value}')";
    }
}
