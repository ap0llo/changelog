using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace schema.Model
{
    internal class JsonSchema
    {
        private const string s_MetaSchema = "http://json-schema.org/draft-04/schema#";


        private readonly Dictionary<string, JsonSchemaDefinition> m_Definitions = new Dictionary<string, JsonSchemaDefinition>();


        public JsonSchemaTypeReference RootObjectType { get; }

        public IReadOnlyDictionary<string, JsonSchemaDefinition> Definitions => m_Definitions;


        public JsonSchema(JsonSchemaTypeReference rootObjectType)
        {
            RootObjectType = rootObjectType ?? throw new ArgumentNullException(nameof(rootObjectType));
        }


        public JsonSchema AddDefinition(JsonSchemaDefinition definition)
        {
            if (definition is null)
                throw new ArgumentNullException(nameof(definition));

            //TODO: Check if definition already exists
            m_Definitions.Add(definition.Name, definition);
            return this;
        }

        public JObject ToJson()
        {
            var json = new JObject(
                new JProperty("$schema", s_MetaSchema),
                new JProperty("$ref", RootObjectType.Ref)
            );

            if (Definitions.Any())
            {
                json.Add(
                    new JProperty("definitions", new JObject(
                        Definitions.Select(kvp => new JProperty(kvp.Key, kvp.Value.Type.ToJson()))
                    )));
            }

            return json;
        }
    }
}
