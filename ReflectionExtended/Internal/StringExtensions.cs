namespace ReflectionExtended.Internal
{
    internal static class StringExtensions
    {
        public static bool Contains(this string self, string value, bool ignoreCase)
        {
            return !ignoreCase ? self.Contains(value) : self.ToLower().Contains(value.ToLower());
        }
    }
}
