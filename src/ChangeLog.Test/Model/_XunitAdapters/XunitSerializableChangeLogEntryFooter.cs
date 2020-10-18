using System;
using Grynwald.ChangeLog.ConventionalCommits;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Test.Model.Text;
using Xunit.Abstractions;

namespace Grynwald.ChangeLog.Test.Model
{
    /// <summary>
    /// Wrapper class to make <see cref="ChangeLogEntryFooter"/> serializable by xunit
    /// </summary>
    public sealed class XunitSerializableChangeLogEntryFooter : IXunitSerializable
    {
        internal ChangeLogEntryFooter Value { get; private set; }


        internal XunitSerializableChangeLogEntryFooter(ChangeLogEntryFooter value) => Value = value;


        [Obsolete("For use by Xunit only", true)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public XunitSerializableChangeLogEntryFooter()
        { }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.


        public void Deserialize(IXunitSerializationInfo info)
        {
            var name = info.GetValue<string>(nameof(ChangeLogEntryFooter.Name));
            var value = info.GetValue<XunitSerializableTextElement>(nameof(ChangeLogEntryFooter.Value));

            Value = new ChangeLogEntryFooter(new CommitMessageFooterName(name), value.Value);
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue(nameof(Value.Name), Value.Name.Value);
            info.AddValue(nameof(Value.Value), new XunitSerializableTextElement(Value.Value));
        }

        internal static XunitSerializableChangeLogEntryFooter Wrap(ChangeLogEntryFooter value) => new XunitSerializableChangeLogEntryFooter(value);
    }
}
