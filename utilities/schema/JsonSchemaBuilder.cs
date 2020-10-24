using System;
using System.Collections.Generic;
using System.Linq;
using schema.Model;

namespace schema
{
    internal class JsonSchemaBuilder<T> where T : class
    {
        private readonly Dictionary<Type, (int index, JsonSchemaDefinition definition)> m_Definitions = new Dictionary<Type, (int, JsonSchemaDefinition)>();
        private readonly JsonSchemaTypeReference m_RootObjectType;

        public JsonSchema Schema
        {
            get
            {
                var schema = new JsonSchema(m_RootObjectType);
                foreach (var (_, definition) in m_Definitions.Values.OrderByDescending(x => x.index))
                {
                    schema.AddDefinition(definition);
                }
                return schema;
            }
        }


        public JsonSchemaBuilder()
        {
            var schemaType = GetSchemaType(typeof(T));

            if (schemaType is JsonSchemaTypeReference reference)
            {
                m_RootObjectType = reference;
            }
            else
            {
                throw new NotImplementedException();
            }
        }


        private JsonSchemaType GetSchemaType(Type runtimeType)
        {
            if (runtimeType == typeof(string))
            {
                return JsonSchemaPrimitiveType.String;
            }
            else if (runtimeType == typeof(int) || runtimeType == typeof(int?))
            {
                return JsonSchemaPrimitiveType.Integer;
            }
            else if (runtimeType.IsGenericType && typeof(Dictionary<,>).IsAssignableFrom(runtimeType.GetGenericTypeDefinition()))
            {
                return GetDictionaryType(runtimeType);
            }
            else if (runtimeType.IsEnum)
            {
                return GetEnumType(runtimeType);
            }
            else if (runtimeType.IsArray)
            {
                return GetArrayType(runtimeType);
            }
            else if (runtimeType.IsClass)
            {
                return GetObjectType(runtimeType);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private JsonSchemaType GetObjectType(Type runtimeType)
        {
            if (m_Definitions.TryGetValue(runtimeType, out var existingDefinition))
                return existingDefinition.definition.Reference;

            var schemaType = new JsonSchemaObjectType();

            foreach (var runtimeProperty in runtimeType.GetProperties())
            {
                schemaType.AddProperty(
                    ToCamelCase(runtimeProperty.Name),
                    GetSchemaType(runtimeProperty.PropertyType)
                );
            }

            var definition = new JsonSchemaDefinition(ToCamelCase(runtimeType.Name), schemaType);
            m_Definitions.Add(runtimeType, (m_Definitions.Count + 1, definition));

            return definition.Reference;
        }

        private JsonSchemaType GetDictionaryType(Type runtimeType)
        {
            var runtimeKeyType = runtimeType.GetGenericArguments()[0];

            if (runtimeKeyType != typeof(string))
                throw new NotImplementedException();

            var valueSchemaType = GetSchemaType(runtimeType.GetGenericArguments()[1]);

            var schemaType = new JsonSchemaObjectType();
            schemaType.AddPatternProperty(".*", valueSchemaType);

            return schemaType;
        }

        private JsonSchemaType GetEnumType(Type runtimeTime)
        {
            if (!runtimeTime.IsEnum)
                throw new InvalidOperationException();

            var values = Enum.GetValues(runtimeTime).Cast<object>().Select(x => x.ToString()!);
            return new JsonSchemaEnumType(values);
        }

        private JsonSchemaType GetArrayType(Type runtimeType)
        {
            if (!runtimeType.IsArray)
                throw new InvalidOperationException();

            var schemaElementType = GetSchemaType(runtimeType.GetElementType()!);
            return new JsonSchemaArrayType(schemaElementType);
        }

        private static string ToCamelCase(string name)
        {
            //TODO: Handle edge-cases

            if (String.IsNullOrWhiteSpace(name))
                return name;

            return Char.ToLower(name[0]) + name.Substring(1);
        }
    }
}
