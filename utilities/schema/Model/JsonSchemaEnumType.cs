using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace schema.Model
{
    internal class JsonSchemaEnumType : JsonSchemaType
    {
        private readonly string[] m_Values;


        public IReadOnlyList<string> Values => m_Values;


        public JsonSchemaEnumType(IEnumerable<string> values)
        {
            m_Values = values.ToArray();
        }


        public override JObject ToJson()
        {
            var json = JsonSchemaPrimitiveType.String.ToJson();

            json.Add(
                new JProperty("enum", new JArray(m_Values))
            );

            return json;
        }
    }
}
