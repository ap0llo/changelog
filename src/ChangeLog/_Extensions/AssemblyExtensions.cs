using System;
using System.IO;
using System.Reflection;

namespace Grynwald.ChangeLog
{
    internal static class AssemblyExtensions
    {
        public static string ReadEmbeddedResource(this Assembly assembly, string resourceName)
        {
            using var resourceStream = assembly.GetManifestResourceStream(resourceName);

            if (resourceStream is null)
                throw new InvalidOperationException($"Resource '{resourceName}' does not exist in assembly '{assembly.GetName().Name}'");

            using var streamReader = new StreamReader(resourceStream);

            return streamReader.ReadToEnd();
        }
    }
}
