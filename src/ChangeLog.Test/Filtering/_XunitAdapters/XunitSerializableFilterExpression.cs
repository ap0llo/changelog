using System;
using Grynwald.ChangeLog.Filtering;
using Xunit.Abstractions;

namespace Grynwald.ChangeLog.Test.Filtering
{
    /// <summary>
    /// Wrapper class to make <see cref="FilterExpression"/> serializable by xunit
    /// </summary>
    public sealed class XunitSerializableFilterExpression : IXunitSerializable
    {
        internal FilterExpression Value { get; private set; }


        internal XunitSerializableFilterExpression(FilterExpression value) => Value = value;


        [Obsolete("For use by Xunit only", true)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public XunitSerializableFilterExpression()
        { }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.


        public void Deserialize(IXunitSerializationInfo info)
        {
            var type = info.GetValue<string>(nameof(FilterExpression.Type));
            var scope = info.GetValue<string>(nameof(FilterExpression.Scope));

            Value = new FilterExpression(type, scope);
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue(nameof(Value.Type), Value.Type);
            info.AddValue(nameof(Value.Scope), Value.Scope);
        }


        internal static XunitSerializableFilterExpression Wrap(FilterExpression value) => new XunitSerializableFilterExpression(value);
    }
}
