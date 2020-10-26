using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Grynwald.ChangeLog.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace schema
{
    public class JsonSchemaGenerator
    {
        private const string s_SchemaNamespace = "http://json-schema.org/draft-04/schema#";

        private static readonly JsonSerializerSettings s_JsonSerializerSettings = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
            ContractResolver = new DefaultContractResolver()
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            },
            Converters = new[]
            {
                new StringEnumConverter()
            }
        };
        private static readonly Dictionary<Type, Func<JObject>> s_KnownTypes = new Dictionary<Type, Func<JObject>>()
        {
            { typeof(string), () => new JObject(new JProperty("type", "string"))  },
            { typeof(int),    () => new JObject(new JProperty("type", "integer")) },
            { typeof(bool),   () => new JObject(new JProperty("type", "boolean")) },
        };


        public static JObject GetSchema<T>() where T : class
        {
            var schema = GetTypeDefinition(typeof(T), includeDefault: false, null);
            schema.AddFirst(new JProperty("$schema", s_SchemaNamespace));
            return schema;
        }

        public static JObject GetSchema<T>(T? defaultValue) where T : class
        {
            var schema = GetTypeDefinition(typeof(T), includeDefault: true, defaultValue);
            schema.AddFirst(new JProperty("$schema", s_SchemaNamespace));
            return schema;
        }


        private static JObject GetTypeDefinition(Type runtimeType, bool includeDefault, object? defaultValue)
        {
            if (s_KnownTypes.TryGetValue(runtimeType, out var knownTypeProvider))
            {
                return knownTypeProvider();
            }
            else if (runtimeType.IsNullableValueType(out var valueType))
            {
                var typeDefinition = GetTypeDefinition(valueType, includeDefault, null);

                // for nullable enum types, add "null" to the list of allowed values
                if (valueType.IsEnum)
                {
                    typeDefinition
                        .GetOrAddProperty("enum", () => new JArray())
                        .AddFirst(SerializeValue(null));
                }

                return typeDefinition;
            }
            else if (runtimeType.IsEnum)
            {
                // Map enum values to strings with an enumeration of valid values
                var enumValues = Enum.GetValues(runtimeType).Cast<object>().Select(x => x.ToString()!).ToArray();
                return new JObject()
                    .WithProperty("type", "string")
                    .WithProperty("enum", new JArray(enumValues));
            }
            else if (runtimeType.IsArrayType(out var arrayElementType))
            {
                return new JObject()
                    .WithProperty("type", "array")
                    .WithProperty("items", GetTypeDefinition(arrayElementType, false, null));
            }
            else if (runtimeType.IsDictionary(out var keyType, out var elementType) && keyType == typeof(string))
            {
                // dictionaries are mapped as JSON objects with the keys being used as property names
                // => use "patternProperties" to define "any" property.
                // and get a schema definition for the dictionary values
                return new JObject()
                    .WithProperty("type", "object")
                    .WithProperty(
                        "patternProperties",
                        new JObject().WithProperty(
                            ".*",
                            GetTypeDefinition(runtimeType.GetGenericArguments()[1], includeDefault, null)
                ));
            }
            else
            {
                // all other types are mapped to JSON object recursively

                var objectDefinition = new JObject().WithProperty("type", "object");

                foreach (var runtimeProperty in runtimeType.GetProperties().Where(p => !p.HasCustomAttribute<JsonSchemaIgnoreAttribute>()))
                {
                    var propertyDefaultValue = defaultValue is null
                        ? default
                        : runtimeProperty.GetValue(defaultValue);

                    var propertyDefinition = GetPropertyDefinition(runtimeProperty, includeDefault, propertyDefaultValue);

                    objectDefinition
                        .GetOrAddProperty("properties", () => new JObject())
                        .Add(propertyDefinition);
                }

                return objectDefinition;
            }
        }

        private static JProperty GetPropertyDefinition(PropertyInfo runtimeProperty, bool includeDefault, object? defaultValue)
        {
            var propertyName = GetPropertyName(runtimeProperty);
            var propertyDefinition = GetTypeDefinition(runtimeProperty.PropertyType, includeDefault, defaultValue);

            if (includeDefault && runtimeProperty.HasCustomAttribute<JsonSchemaDefaultValueAttribute>())
            {
                // Special handling for Dictionary properties:
                // Instead of serializing the entire dictionary as a single JSON object and include it in the schema as default value,
                // "flatten" the dictionary into individual properties and then include the dictionary values
                // as default values for the individual properties.
                // This makes more sense when generating the schema for a configuration file,
                // because in the configuration file, typically only some of the keys get overridden
                //
                // e.g. the following class
                //      public class Class1
                //      {
                //          public Dictionary<string, string> Property1 {get;set;} = new Dictionary<string, string>()
                //          {
                //              { "key1", "value1" },
                //              { "key2", "value2" },
                //          }:
                //      }
                //
                // gets mapped to this JSON schema
                //      {
                //        "$schema": "http://json-schema.org/draft-04/schema#",
                //        "type": "object",
                //        "properties": {
                //          "property1": {
                //            "type": "object",
                //            "properties": {
                //              "key1": {
                //                "type": "string",
                //                "default": "value1"
                //              },
                //              "key2": {
                //                "type": "string",
                //                "default": "value2"
                //              }
                //            },
                //            "patternProperties": {
                //              ".*" :{
                //                "type" : "string"
                //              }
                //            }
                //          }
                //        }
                //      }


                if (runtimeProperty.PropertyType.IsDictionary(out var keyType, out _) && keyType == typeof(string))
                {
                    var defaultValueProperties = GetDictionaryDefaultValues((IDictionary)defaultValue!);
                    if (defaultValueProperties.Any())
                    {
                        propertyDefinition.WithProperty(
                            "properties",
                            new JObject(defaultValueProperties)
                        );
                    }
                }
                else
                {
                    propertyDefinition.WithProperty("default", SerializeValue(defaultValue));
                }
            }

            // Add "uniqueItems" : true if property has a JsonSchemaUniqueItems attribute
            // (only applies to arrays)
            if (runtimeProperty.PropertyType.IsArray &&
                runtimeProperty.HasCustomAttribute<JsonSchemaUniqueItemsAttribute>())
            {
                propertyDefinition.WithProperty("uniqueItems", true);
            }

            return new JProperty(propertyName, propertyDefinition);
        }

        private static JToken SerializeValue(object? value)
        {
            if (value is string stringValue && String.IsNullOrEmpty(stringValue))
            {
                value = null;
            }

            var json = JsonConvert.SerializeObject(value, s_JsonSerializerSettings);
            return JToken.Parse(json);
        }

        private static IEnumerable<JProperty> GetDictionaryDefaultValues(IDictionary dictionary)
        {
            foreach (var key in dictionary.Keys.Cast<string>().OrderBy(x => x))
            {
                var value = dictionary[key!];
                yield return new JProperty(
                    key!,
                    GetTypeDefinition(value!.GetType(), false, null).WithProperty("default", SerializeValue(value)));
            }
        }

        private static string GetPropertyName(PropertyInfo runtimeProperty)
        {
            var nameAttribute = runtimeProperty.GetCustomAttribute<JsonSchemaPropertyNameAttribute>();
            if (nameAttribute != null)
                return nameAttribute.Name;

            return Char.ToLower(runtimeProperty.Name[0]) + runtimeProperty.Name.Substring(1);
        }
    }
}
