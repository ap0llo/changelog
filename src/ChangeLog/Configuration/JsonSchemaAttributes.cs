// Attributes used to auto-generate the configuration file schema
using System;

namespace Grynwald.ChangeLog.Configuration
{
    /// <summary>
    /// Indicates that a property's value should be included as <c>default</c> in the JSON schema
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    internal class JsonSchemaDefaultValueAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    internal class JsonSchemaUniqueItemsAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    internal class JsonSchemaIgnoreAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    internal class JsonSchemaPropertyNameAttribute : Attribute
    {
        public string Name { get; }

        public JsonSchemaPropertyNameAttribute(string name)
        {
            Name = name;
        }
    }
}
