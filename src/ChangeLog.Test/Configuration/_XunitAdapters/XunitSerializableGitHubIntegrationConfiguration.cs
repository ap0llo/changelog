using System;
using Grynwald.ChangeLog.Configuration;
using Xunit.Abstractions;

namespace Grynwald.ChangeLog.Test.Configuration
{
    /// <summary>
    /// Wrapper class to make <see cref="ChangeLogConfiguration.GitHubIntegrationConfiguration"/> serializable by xunit
    /// </summary>
    public class XunitSerializableGitHubIntegrationConfiguration : IXunitSerializable
    {
        internal ChangeLogConfiguration.GitHubIntegrationConfiguration Value { get; private set; }


        internal XunitSerializableGitHubIntegrationConfiguration(ChangeLogConfiguration.GitHubIntegrationConfiguration value) => Value = value;

        [Obsolete("For use by Xunit only", true)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public XunitSerializableGitHubIntegrationConfiguration()
        { }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.


        public void Deserialize(IXunitSerializationInfo info)
        {
            Value = new ChangeLogConfiguration.GitHubIntegrationConfiguration()
            {
                AccessToken = info.GetValue<string>(nameof(Value.AccessToken)),
                RemoteName = info.GetValue<string>(nameof(Value.RemoteName)),
                Host = info.GetValue<string>(nameof(Value.Host)),
                Owner = info.GetValue<string>(nameof(Value.Owner)),
                Repository = info.GetValue<string>(nameof(Value.Repository)),
            };
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue(nameof(Value.AccessToken), Value.AccessToken);
            info.AddValue(nameof(Value.RemoteName), Value.RemoteName);
            info.AddValue(nameof(Value.Host), Value.Host);
            info.AddValue(nameof(Value.Owner), Value.Owner);
            info.AddValue(nameof(Value.Repository), Value.Repository);
        }


        public static implicit operator ChangeLogConfiguration.GitHubIntegrationConfiguration(XunitSerializableGitHubIntegrationConfiguration config) => config.Value;

    }
}
