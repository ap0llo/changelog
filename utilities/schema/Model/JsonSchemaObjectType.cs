using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace schema.Model
{
    internal class JsonSchemaObjectType : JsonSchemaType
    {
        private readonly Dictionary<string, JsonSchemaType> m_Properties = new Dictionary<string, JsonSchemaType>();
        private readonly Dictionary<string, JsonSchemaType> m_PatternProperties = new Dictionary<string, JsonSchemaType>();


        public IReadOnlyDictionary<string, JsonSchemaType> Properties => m_Properties;

        public IReadOnlyDictionary<string, JsonSchemaType> PatternProperties => m_PatternProperties;


        public void AddProperty(string name, JsonSchemaType type)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentException($"'{nameof(name)}' cannot be null or whitespace", nameof(name));

            if (type is null)
                throw new ArgumentNullException(nameof(type));

            //TODO: check if property already exits

            m_Properties.Add(name, type);
        }

        public void AddPatternProperty(string pattern, JsonSchemaType type)
        {
            if (String.IsNullOrWhiteSpace(pattern))
                throw new ArgumentException($"'{nameof(pattern)}' cannot be null or whitespace", nameof(pattern));

            if (type is null)
                throw new ArgumentNullException(nameof(type));

            //TODO: check if property already exits

            m_PatternProperties.Add(pattern, type);
        }


        public override JObject ToJson()
        {
            var json = new JObject(
                new JProperty("type", "object")
            );

            if (Properties.Any())
            {
                json.Add(
                    new JProperty(
                        "properties",
                        new JObject(
                            Properties.Select(kvp => new JProperty(kvp.Key, kvp.Value.ToJson()))
                )));
            }

            if (PatternProperties.Any())
            {
                json.Add(
                    new JProperty(
                        "properties",
                        new JObject(
                            PatternProperties.Select(kvp => new JProperty(kvp.Key, kvp.Value.ToJson()))
                )));
            }

            return json;
        }
    }
}
