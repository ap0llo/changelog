using ChangeLogCreator.ConventionalCommits;
using Xunit.Abstractions;

namespace ChangeLogCreator.Test.ConventionalCommits
{
    /// <summary>
    /// Wrapper class to make <see cref="HeaderToken"/> serializable by xunit
    /// </summary>
    public class XunitSerializableHeaderToken : IXunitSerializable
    {
        internal HeaderToken Value { get; private set; }


        internal XunitSerializableHeaderToken(HeaderToken value) => Value = value;

        // parameterless constructor required by Xunit
        public XunitSerializableHeaderToken()
        { }


        public void Deserialize(IXunitSerializationInfo info)
        {
            var kind = info.GetValue<HeaderTokenKind>(nameof(HeaderToken.Kind));
            var value = info.GetValue<string>(nameof(HeaderToken.Value));
            var lineNumber = info.GetValue<int>(nameof(HeaderToken.LineNumber));
            var columnNumber = info.GetValue<int>(nameof(HeaderToken.ColumnNumber));

            Value = new HeaderToken(kind, value, lineNumber, columnNumber);
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue(nameof(Value.Kind), Value.Kind);
            info.AddValue(nameof(Value.Value), Value.Value);
            info.AddValue(nameof(Value.LineNumber), Value.LineNumber);
            info.AddValue(nameof(Value.ColumnNumber), Value.ColumnNumber);
        }

        public override string ToString() => Value?.ToString();
    }
}
