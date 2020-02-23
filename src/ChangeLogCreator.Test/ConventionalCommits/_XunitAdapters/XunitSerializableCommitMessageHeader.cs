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
            var type = info.GetValue<string>(nameof(CommitMessageHeader.Type));
            var scope = info.GetValue<string>(nameof(CommitMessageHeader.Scope));
            var isBreakingChange = info.GetValue<bool>(nameof(CommitMessageHeader.IsBreakingChange));
            var description = info.GetValue<string>(nameof(CommitMessageHeader.Description));

            Value = new CommitMessageHeader(type: type, description: description, scope: scope, isBreakingChange: isBreakingChange);
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
