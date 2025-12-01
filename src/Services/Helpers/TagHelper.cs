namespace Services.Helpers
{
    public static class TagHelper
    {
        /// <summary>
        /// Adds a game tag to the specified dictionary by parsing a line of text.
        /// </summary>
        /// <remarks>The tag key is converted to lowercase and trimmed of square brackets. The tag value
        /// is trimmed of square brackets and double quotes. If the tag key already exists in the dictionary, its value
        /// will be overwritten.</remarks>
        /// <param name="tagDictionary">The dictionary to which the parsed tag will be added. The key is the tag name, and the value is the tag's
        /// associated value.</param>
        /// <param name="line">A string representing the tag in the format "[TagKey] TagValue". The tag key is extracted from the first
        /// part of the string, and the tag value is extracted from the second part.</param>
        public static void AddGameTag(Dictionary<string, string> tagDictionary, string line)
        {
            var tagSections = line.Split(" ", 2);

            string tagKey = tagSections[0].Trim('[').ToLower();

            string tagValue = tagSections[1].Trim(']').Replace("\"", "");

            tagDictionary[tagKey] = tagValue;
        }
    }
}
