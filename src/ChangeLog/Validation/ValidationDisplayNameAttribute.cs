using System;

namespace Grynwald.ChangeLog.Validation
{
    /// <summary>
    /// Defines a property's display name in validation error messages
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    internal sealed class ValidationDisplayNameAttribute : Attribute
    {
        public string DisplayName { get; }

        public ValidationDisplayNameAttribute(string displayName)
        {
            DisplayName = displayName;
        }
    }
}
