using Newtonsoft.Json.Linq;

namespace schema.Model
{
    internal abstract class JsonSchemaType
    {
        public abstract JObject ToJson();
    }
}
