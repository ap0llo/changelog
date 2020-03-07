using System;
using System.Collections.Generic;
using System.Text;

namespace ChangeLogCreator.Configuration
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple =false, Inherited = true)]
    internal sealed class ConfigurationValueAttribute : Attribute
    {
        public string Key { get; }

        public ConfigurationValueAttribute(string key)
        {
            Key = key;
        }
    }
}
