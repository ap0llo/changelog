using System;
using Newtonsoft.Json.Linq;

namespace schema
{
    internal static class JsonExtensions
    {
        public static T GetOrAddProperty<T>(this JObject jobject, string propertyName, Func<T> factory) where T : JToken
        {
            var property = jobject.Property(propertyName);
            if (property is null)
            {
                var value = factory();
                jobject.Add(new JProperty(propertyName, value));
                return value;
            }
            else
            {
                return (T)property.Value;
            }
        }

        public static JObject WithProperty(this JObject jobject, string propertyName, object propertyValue)
        {
            jobject.Add(new JProperty(propertyName, propertyValue));
            return jobject;
        }
    }
}
