using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace schema
{
    internal static class ReflectionExtensions
    {
        public static bool IsNullableValueType(this Type runtimeType, [NotNullWhen(true)] out Type? valueType)
        {
            valueType = default;

            if (!runtimeType.IsGenericType)
                return false;

            if (!typeof(Nullable<>).IsAssignableFrom(runtimeType.GetGenericTypeDefinition()))
                return false;

            valueType = runtimeType.GetGenericArguments()[0];
            return true;
        }

        public static bool IsArrayType(this Type type, [NotNullWhen(true)] out Type? elementType)
        {
            if (type.IsArray)
            {
                elementType = type.GetElementType() ?? throw new InvalidOperationException();
                return true;
            }
            else
            {
                elementType = default;
                return false;
            }
        }

        public static bool IsDictionary(this Type runtimeType, [NotNullWhen(true)] out Type? keyType, [NotNullWhen(true)] out Type? valueType)
        {
            //TODO: IDictionary, IReadOnlyDictionary

            keyType = default;
            valueType = default;

            if (!runtimeType.IsGenericType)
                return false;

            if (!typeof(Dictionary<,>).IsAssignableFrom(runtimeType.GetGenericTypeDefinition()))
                return false;

            var genericArguments = runtimeType.GetGenericArguments();

            keyType = genericArguments[0];
            valueType = genericArguments[1];
            return true;
        }

        public static bool HasCustomAttribute<T>(this MemberInfo member) where T : Attribute => member.GetCustomAttribute<T>() != null;
    }
}
