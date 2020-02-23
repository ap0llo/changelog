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
                Type = info.GetValue<string>(nameof(CommitMessage.Type)),
                Scope = info.GetValue<string>(nameof(CommitMessage.Scope)),
                IsBreakingChange = info.GetValue<bool>(nameof(CommitMessage.IsBreakingChange)),
                Description = info.GetValue<string>(nameof(CommitMessage.Description)),
                Body = info.GetValue<string[]>(nameof(CommitMessage.Body)),
                Footers = info.GetValue<XunitSerializableCommitMessageFooter[]>(nameof(CommitMessage.Footers)).Select(x => x.Value).ToArray(),
            };
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue(nameof(Value.Type), Value.Type);
            info.AddValue(nameof(Value.Scope), Value.Scope);
            info.AddValue(nameof(Value.IsBreakingChange), Value.IsBreakingChange);
            info.AddValue(nameof(Value.Description), Value.Description);
            info.AddValue(nameof(Value.Body), Value.Body.ToArray());
            info.AddValue(nameof(Value.Footers), Value.Footers.Select(x => new XunitSerializableCommitMessageFooter(x)).ToArray());

        }
    }
}
