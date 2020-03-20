using System;
using Grynwald.ChangeLog.ConventionalCommits;
using Xunit.Abstractions;

namespace Grynwald.ChangeLog.Test.ConventionalCommits
{
    /// <summary>
    /// Wrapper class to make <see cref="CommitMessageFooter"/> serializable by xunit
    /// </summary>
    public class XunitSerializableCommitMessageFooter : IXunitSerializable
    {
        public CommitMessageFooter Value { get; private set; }


        internal XunitSerializableCommitMessageFooter(CommitMessageFooter value) => Value = value;


        [Obsolete("For use by Xunit only", true)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public XunitSerializableCommitMessageFooter()
        { }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.


        public void Deserialize(IXunitSerializationInfo info)
        {
            var type = info.GetValue<string>(nameof(CommitMessageFooter.Name));
            var description = info.GetValue<string>(nameof(CommitMessageFooter.Value));

            Value = new CommitMessageFooter(new CommitMessageFooterName(type), description);            
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue(nameof(Value.Name), Value.Name.Value);
            info.AddValue(nameof(Value.Value), Value.Value);
        }
    }
}
