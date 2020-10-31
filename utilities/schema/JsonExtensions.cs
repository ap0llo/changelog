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

        /// <summary>
        /// Adds the specified value as property to the JSON object.
        /// If the property already exists, the property value is converted to an array which contains both the existing and the new value.
        /// If the value already is an array, the new value is appended to the array.
        /// </summary>
        public static void AddPropertyValue(this JObject jobject, string propertyName, object newValue)
        {
            var property = jobject.Property(propertyName);

            if (property is null)
            {
                jobject.Add(new JProperty(propertyName, newValue));
            }
            else
            {
                var value = property.Value;

                if (value is JArray arrayValue)
                {
                    arrayValue.Add(newValue);
                }
                else
                {
                    property.Value = new JArray(property.Value, newValue);
                }
            }
        }
    }
}
