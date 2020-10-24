using System;
using Newtonsoft.Json.Linq;

namespace schema.Model
{
    internal class JsonSchemaArrayType : JsonSchemaType
    {
        public JsonSchemaType ItemType { get; }


        public JsonSchemaArrayType(JsonSchemaType itemType)
        {
            ItemType = itemType ?? throw new ArgumentNullException(nameof(itemType));
        }


        public override JObject ToJson()
        {
            return new JObject(
                new JProperty("type", "array"),
                new JProperty("items", ItemType.ToJson())
            );
        }
    }
}
