using System;
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


        [Obsolete("For use by Xunit only", true)]
        public XunitSerializableCommitMessageFooter()
        { }


        public void Deserialize(IXunitSerializationInfo info)
        {
            var type = info.GetValue<string>(nameof(CommitMessageFooter.Name));
            var description = info.GetValue<string>(nameof(CommitMessageFooter.Value));

            Value = new CommitMessageFooter(new CommitMessageFooterName(type), description);            
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue(nameof(Value.Name), Value.Name.Key);
            info.AddValue(nameof(Value.Value), Value.Value);
        }
    }
}
