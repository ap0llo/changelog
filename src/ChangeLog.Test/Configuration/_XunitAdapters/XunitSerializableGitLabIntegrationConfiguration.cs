using System;
using Grynwald.ChangeLog.Configuration;
using Xunit.Abstractions;

namespace Grynwald.ChangeLog.Test.Configuration
{
    /// <summary>
    /// Wrapper class to make <see cref="ChangeLogConfiguration.GitLabIntegrationConfiguration"/> serializable by xunit
    /// </summary>
    public class XunitSerializableGitLabIntegrationConfiguration : IXunitSerializable
    {
        internal ChangeLogConfiguration.GitLabIntegrationConfiguration Value { get; private set; }


        internal XunitSerializableGitLabIntegrationConfiguration(ChangeLogConfiguration.GitLabIntegrationConfiguration value) => Value = value;

        [Obsolete("For use by Xunit only", true)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public XunitSerializableGitLabIntegrationConfiguration()
        { }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.


        public void Deserialize(IXunitSerializationInfo info)
        {
            Value = new ChangeLogConfiguration.GitLabIntegrationConfiguration()
            {
                AccessToken = info.GetValue<string>(nameof(Value.AccessToken)),
                RemoteName = info.GetValue<string>(nameof(Value.RemoteName)),
                Host = info.GetValue<string>(nameof(Value.Host)),
                Namespace = info.GetValue<string>(nameof(Value.Namespace)),
                Project = info.GetValue<string>(nameof(Value.Project)),
            };
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue(nameof(Value.AccessToken), Value.AccessToken);
            info.AddValue(nameof(Value.RemoteName), Value.RemoteName);
            info.AddValue(nameof(Value.Host), Value.Host);
            info.AddValue(nameof(Value.Namespace), Value.Namespace);
            info.AddValue(nameof(Value.Project), Value.Project);
        }


        public static implicit operator ChangeLogConfiguration.GitLabIntegrationConfiguration(XunitSerializableGitLabIntegrationConfiguration config) => config.Value;

    }
}
