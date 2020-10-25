using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace schema
{
    public class JsonSchemaBuilder
    {
        private static readonly Dictionary<Type, JObject> s_KnownTypes = new Dictionary<Type, JObject>()
        {
            { typeof(string), new JObject(new JProperty("type", "string")) },
            { typeof(int), new JObject(new JProperty("type", "integer")) },
            { typeof(bool), new JObject(new JProperty("type", "boolean")) },
        };

        private const string s_SchemaNamespace = "http://json-schema.org/draft-04/schema#";


        public static JObject GetSchema<T>()
        {
            //TODO: Ensure T is a object

            var schema = GetTypeDefinition(typeof(T));
            schema.AddFirst(new JProperty("$schema", s_SchemaNamespace));
            return schema;
        }


        private static JObject GetTypeDefinition(Type runtimeType)
        {
            if (s_KnownTypes.TryGetValue(runtimeType, out var knownTypeDefinition))
            {
                return knownTypeDefinition;
            }
            else if (IsNullableEnum(runtimeType, out var valueType))
            {
                return GetTypeDefinition(valueType);
            }
            else if (runtimeType.IsEnum)
            {
                var enumValues = Enum.GetValues(runtimeType).Cast<object>().Select(x => x.ToString()!).ToArray();
                return new JObject(
                    new JProperty("type", "string"),
                    new JProperty("enum", new JArray(enumValues))
                );
            }
            else if (runtimeType.IsArray)
            {
                return new JObject(
                    new JProperty("type", "array"),
                    new JProperty("items", GetTypeDefinition(runtimeType.GetElementType()!))
                );
            }
            else if (IsConvertibleDictionary(runtimeType, out var elementType))
            {
                return new JObject(
                    new JProperty("type", "object"),
                    new JProperty("patternProperties", new JObject(
                        new JProperty(".*", GetTypeDefinition(runtimeType.GetGenericArguments()[1]))
                )));
            }
            else
            {
                var objectDefinition = new JObject(new JProperty("type", "object"));

                JObject? propertiesDefinition = default;

                foreach (var runtimeProperty in runtimeType.GetProperties())
                {
                    propertiesDefinition ??= new JObject();
                    propertiesDefinition.Add(
                        new JProperty(
                            ToCamelCase(runtimeProperty.Name),
                            GetTypeDefinition(runtimeProperty.PropertyType)
                    ));
                }

                if (propertiesDefinition != null)
                    objectDefinition.Add(new JProperty("properties", propertiesDefinition));

                return objectDefinition;
            }

        }


        private static string ToCamelCase(string name)
        {
            //TODO: Handle edge-cases

            if (String.IsNullOrWhiteSpace(name))
                return name;

            return Char.ToLower(name[0]) + name.Substring(1);
        }


        private static bool IsConvertibleDictionary(Type runtimeType, [NotNullWhen(true)] out Type? elementType)
        {
            elementType = default;

            if (!runtimeType.IsGenericType)
                return false;

            if (!typeof(Dictionary<,>).IsAssignableFrom(runtimeType.GetGenericTypeDefinition()))
                return false;

            var genericArguments = runtimeType.GetGenericArguments();

            if (genericArguments[0] != typeof(string))
                return false;

            elementType = runtimeType.GetGenericArguments()[1];
            return true;
        }

        private static bool IsNullableEnum(Type runtimeType, [NotNullWhen(true)] out Type? valueType)
        {
            valueType = default;

            if (!runtimeType.IsGenericType)
                return false;

            if (!typeof(Nullable<>).IsAssignableFrom(runtimeType.GetGenericTypeDefinition()))
                return false;

            valueType = runtimeType.GetGenericArguments()[0];
            return true;
        }
    }
}
