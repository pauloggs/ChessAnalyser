using System.Text.RegularExpressions;

namespace Services.Helpers
{
    public static class StringExtensions
    {
        /// <summary>
        /// Removes all line endings from the specified string and replaces them with a single space.
        /// </summary>
        /// <param name="input">The input string from which to remove line endings. Cannot be <see langword="null"/>.</param>
        /// <returns>A new string with all line endings replaced by a single space. If <paramref name="input"/> is empty, the
        /// method returns an empty string.</returns>
        public static string RemoveLineEndings(this string input) => Regex.Replace(input, @"\r?\n", " ");
    }
}
