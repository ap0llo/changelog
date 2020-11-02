using System;

namespace Grynwald.ChangeLog.Configuration
{
    /// <summary>
    /// Defines a settings's display name for validation and JSON schema generation
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    internal sealed class SettingDisplayNameAttribute : Attribute
    {
        public string DisplayName { get; }

        public SettingDisplayNameAttribute(string displayName)
        {
            DisplayName = displayName;
        }
    }
}
