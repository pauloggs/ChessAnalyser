using System.Text.RegularExpressions;

namespace Services.Helpers
{
    public static class StringExtensions
    {
        public static string RemoveLineEndings(this string input) => Regex.Replace(input, @"\r?\n", " ");
    }
}
