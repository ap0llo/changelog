using System;
using System.Linq;
using Grynwald.ChangeLog.ConventionalCommits;
using Xunit.Abstractions;

namespace Grynwald.ChangeLog.Test.ConventionalCommits
{
    /// <summary>
    /// Wrapper class to make <see cref="CommitMessage"/> serializable by xunit
    /// </summary>
    public class XunitSerializableCommitMessage : IXunitSerializable
    {
        internal CommitMessage Value { get; private set; }


        internal XunitSerializableCommitMessage(CommitMessage value) => Value = value
                ;

        [Obsolete("For use by Xunit only", true)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public XunitSerializableCommitMessage()
        { }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.


        public void Deserialize(IXunitSerializationInfo info)
        {
            var header = info.GetValue<XunitSerializableCommitMessageHeader>(nameof(CommitMessage.Header)).Value;
            var body = info.GetValue<string[]>(nameof(CommitMessage.Body));
            var footers = info.GetValue<XunitSerializableCommitMessageFooter[]>(nameof(CommitMessage.Footers)).Select(x => x.Value).ToArray();

            Value = new CommitMessage(header, body, footers);
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue(nameof(Value.Header), new XunitSerializableCommitMessageHeader(Value.Header));
            info.AddValue(nameof(Value.Body), Value.Body.ToArray());
            info.AddValue(nameof(Value.Footers), Value.Footers.Select(x => new XunitSerializableCommitMessageFooter(x)).ToArray());
        }
    }
}
