using System.Linq;
using ChangeLogCreator.ConventionalCommits;
using Xunit.Abstractions;

namespace ChangeLogCreator.Test.ConventionalCommits
{
    /// <summary>
    /// Wrapper class to make <see cref="CommitMessage"/> serializable by xunit
    /// </summary>
    public class XunitSerializableCommitMessage : IXunitSerializable
    {
        internal CommitMessage Value { get; private set; }


        internal XunitSerializableCommitMessage(CommitMessage value) => Value = value
                ;

        // parameterless constructor required by Xunit
        public XunitSerializableCommitMessage()
        { }


        public void Deserialize(IXunitSerializationInfo info)
        {
            Value = new CommitMessage()
            {
                Header = info.GetValue<XunitSerializableCommitMessageHeader>(nameof(CommitMessage.Header)).Value,
                Body = info.GetValue<string[]>(nameof(CommitMessage.Body)),
                Footers = info.GetValue<XunitSerializableCommitMessageFooter[]>(nameof(CommitMessage.Footers)).Select(x => x.Value).ToArray(),
            };
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue(nameof(Value.Header), new XunitSerializableCommitMessageHeader(Value.Header));
            info.AddValue(nameof(Value.Body), Value.Body.ToArray());
            info.AddValue(nameof(Value.Footers), Value.Footers.Select(x => new XunitSerializableCommitMessageFooter(x)).ToArray());
        }
    }
}
