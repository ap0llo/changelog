using System;

namespace schema.Model
{
    internal class JsonSchemaDefinition
    {
        public string Name { get; }

        public JsonSchemaObjectType Type { get; }

        public JsonSchemaTypeReference Reference => new JsonSchemaTypeReference($"#/definitions/{Name}");


        public JsonSchemaDefinition(string name, JsonSchemaObjectType type)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentException($"'{nameof(name)}' cannot be null or whitespace", nameof(name));

            Name = name;
            Type = type ?? throw new ArgumentNullException(nameof(type));

        }
    }
}
