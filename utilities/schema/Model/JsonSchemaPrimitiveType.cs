using System;
using Newtonsoft.Json.Linq;

namespace schema.Model
{
    internal class JsonSchemaPrimitiveType : JsonSchemaType
    {
        public static readonly JsonSchemaPrimitiveType String = new JsonSchemaPrimitiveType("string");
        public static readonly JsonSchemaPrimitiveType Integer = new JsonSchemaPrimitiveType("integer");

        private readonly string m_Type;


        private JsonSchemaPrimitiveType(string type)
        {
            if (System.String.IsNullOrWhiteSpace(type))
                throw new ArgumentException($"'{nameof(type)}' cannot be null or whitespace", nameof(type));

            m_Type = type;
        }

        public override JObject ToJson() => new JObject(new JProperty("type", m_Type));
    }
}
