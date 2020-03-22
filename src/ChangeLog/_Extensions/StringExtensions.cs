namespace Grynwald.ChangeLog
{
    public static class StringExtensions
    {
        public static string RemoveSuffix(this string value, string suffix)
        {
            return value.EndsWith(suffix)
#if NETCOREAPP2_1
                ? value.Substring(0, value.Length - suffix.Length)
#else
                ? value[..^suffix.Length]
#endif
                : value;
        }

    }
}
