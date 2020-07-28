using System;
using Grynwald.ChangeLog.Git;
using Xunit.Abstractions;

namespace Grynwald.ChangeLog.Test.Git
{
    /// <summary>
    /// Wrapper class to make <see cref="GitRemote"/> serializable by xunit
    /// </summary>
    public class XunitSerializableGitRemote : IXunitSerializable
    {
        internal GitRemote Value { get; private set; }


        internal XunitSerializableGitRemote(GitRemote value) => Value = value;

        [Obsolete("For use by Xunit only", true)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public XunitSerializableGitRemote()
        { }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.


        public void Deserialize(IXunitSerializationInfo info)
        {
            var name = info.GetValue<string>(nameof(GitRemote.Name));
            var url = info.GetValue<string>(nameof(GitRemote.Url));

            Value = new GitRemote(name, url);
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue(nameof(Value.Name), Value.Name);
            info.AddValue(nameof(Value.Url), Value.Url);
        }

        public override string? ToString() => $"(GitRemote: Name = '{Value?.Name}', Url = '{Value?.Url}')";
    }
}
