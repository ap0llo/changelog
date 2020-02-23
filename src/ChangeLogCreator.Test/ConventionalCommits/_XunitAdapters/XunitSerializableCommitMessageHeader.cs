using ChangeLogCreator.ConventionalCommits;
using Xunit.Abstractions;

namespace ChangeLogCreator.Test.ConventionalCommits
{
    /// <summary>
    /// Wrapper class to make <see cref="CommitMessage"/> serializable by xunit
    /// </summary>
    public class XunitSerializableCommitMessageHeader : IXunitSerializable
    {
        internal CommitMessageHeader Value { get; private set; }


        internal XunitSerializableCommitMessageHeader(CommitMessageHeader value) => Value = value
                ;

        // parameterless constructor required by Xunit
        public XunitSerializableCommitMessageHeader()
        { }


        public void Deserialize(IXunitSerializationInfo info)
        {
            Value = new CommitMessageHeader()
            {
                Type = info.GetValue<string>(nameof(CommitMessageHeader.Type)),
                Scope = info.GetValue<string>(nameof(CommitMessageHeader.Scope)),
                IsBreakingChange = info.GetValue<bool>(nameof(CommitMessageHeader.IsBreakingChange)),
                Description = info.GetValue<string>(nameof(CommitMessageHeader.Description)),
            };
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue(nameof(Value.Type), Value.Type);
            info.AddValue(nameof(Value.Scope), Value.Scope);
            info.AddValue(nameof(Value.IsBreakingChange), Value.IsBreakingChange);
            info.AddValue(nameof(Value.Description), Value.Description);
        }
    }
}
