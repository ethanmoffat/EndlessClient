namespace EOLib.Extensions
{
    public static class StringExtensions
    {
        public static string Capitalize(this string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return str;

            if (str.Length == 1)
                return str.ToUpperInvariant();

            return char.ToUpperInvariant(str[0]) + str.Substring(1).ToLowerInvariant();
        }
    }
}
