using ChangeLogCreator.ConventionalCommits;
using Xunit.Abstractions;

namespace ChangeLogCreator.Test.ConventionalCommits
{
    /// <summary>
    /// Wrapper class to make <see cref="CommitMessageFooter"/> serializable by xunit
    /// </summary>
    public class XunitSerializableCommitMessageFooter : IXunitSerializable
    {
        public CommitMessageFooter Value { get; private set; }


        internal XunitSerializableCommitMessageFooter(CommitMessageFooter value) => Value = value;

        // parameterless constructor required by Xunit
        public XunitSerializableCommitMessageFooter()
        { }


        public void Deserialize(IXunitSerializationInfo info)
        {
            var type = info.GetValue<string>(nameof(CommitMessageFooter.Type));
            var description = info.GetValue<string>(nameof(CommitMessageFooter.Description));

            Value = new CommitMessageFooter(type, description);            
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue(nameof(Value.Type), Value.Type);
            info.AddValue(nameof(Value.Description), Value.Description);
        }
    }
}
