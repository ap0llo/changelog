using System;
using Newtonsoft.Json.Linq;

namespace schema.Model
{
    internal class JsonSchemaTypeReference : JsonSchemaType
    {
        public string Ref { get; }


        public JsonSchemaTypeReference(string @ref)
        {
            if (String.IsNullOrWhiteSpace(@ref))
                throw new ArgumentException($"'{nameof(@ref)}' cannot be null or whitespace", nameof(@ref));

            Ref = @ref;
        }


        public override JObject ToJson() => new JObject(new JProperty("$ref", Ref));
    }
}
