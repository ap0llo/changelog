using System.IO;

namespace Grynwald.ChangeLog
{
    internal static class StreamExtensions
    {
        public static string ReadToEnd(this Stream stream)
        {
            using var streamReader = new StreamReader(stream);
            return streamReader.ReadToEnd();
        }
    }
}
