using System.Text;

namespace Grynwald.ChangeLog
{
    internal static class StringBuilderExtensions
    {
        /// <summary>
        /// Gets the current string value for the StringBuilder and clears it.
        /// </summary>
        /// <returns>Returns the StringBuilder's value before it was cleared.</returns>
        public static string GetValueAndClear(this StringBuilder stringBuilder)
        {
            var value = stringBuilder.ToString();
            stringBuilder.Clear();
            return value;
        }
    }
}
