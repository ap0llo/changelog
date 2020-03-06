using System;
using ChangeLogCreator.ConventionalCommits;
using Xunit.Abstractions;

namespace ChangeLogCreator.Test.ConventionalCommits
{
    /// <summary>
    /// Wrapper class to make <see cref="LineToken"/> serializable by xunit
    /// </summary>
    public class XunitSerializableLineToken : IXunitSerializable
    {
        internal LineToken Value { get; private set; }


        internal XunitSerializableLineToken(LineToken value) => Value = value;


        [Obsolete("For use by Xunit only", true)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public XunitSerializableLineToken()
        { }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.


        public void Deserialize(IXunitSerializationInfo info)
        {
            var kind = info.GetValue<LineTokenKind>(nameof(LineToken.Kind));
            var value = info.GetValue<string>(nameof(LineToken.Value));
            var lineNumber = info.GetValue<int>(nameof(LineToken.LineNumber));

            Value = new LineToken(kind, value, lineNumber);
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue(nameof(Value.Kind), Value.Kind);
            info.AddValue(nameof(Value.Value), Value.Value);
            info.AddValue(nameof(Value.LineNumber), Value.LineNumber);
        }

        public override string? ToString() => Value?.ToString();
    }
}
