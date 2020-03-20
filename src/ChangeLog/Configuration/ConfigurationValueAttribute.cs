using System;

namespace Grynwald.ChangeLog.Configuration
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    internal sealed class ConfigurationValueAttribute : Attribute
    {
        public string Key { get; }

        public ConfigurationValueAttribute(string key)
        {
            Key = key;
        }
    }
}
