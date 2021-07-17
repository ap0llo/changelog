namespace Grynwald.ChangeLog
{
    public static class StringExtensions
    {
        public static string RemoveSuffix(this string value, string suffix)
        {
            return value.EndsWith(suffix)
                ? value[..^suffix.Length]
                : value;
        }

    }
}
