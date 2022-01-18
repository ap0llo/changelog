using System;
using System.IO;
using Grynwald.Utilities.IO;

namespace Grynwald.ChangeLog.Test
{
    internal static class TemporaryDirectoryExtensions
    {
        public static string AddSubDirectory(this TemporaryDirectory temporaryDirectory, string relativePath)
        {
            if (Path.IsPathRooted(relativePath))
                throw new ArgumentException("Path must not be rooted", nameof(relativePath));

            var absolutePath = Path.Combine(temporaryDirectory, relativePath);
            Directory.CreateDirectory(absolutePath);

            return absolutePath;
        }

        public static string AddFile(this TemporaryDirectory temporaryDirectory, string relativePath, string contents)
        {
            if (Path.IsPathRooted(relativePath))
                throw new ArgumentException("Path must not be rooted", nameof(relativePath));

            var absolutePath = Path.Combine(temporaryDirectory, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);

            File.WriteAllText(absolutePath, contents);

            return absolutePath;
        }
    }
}
