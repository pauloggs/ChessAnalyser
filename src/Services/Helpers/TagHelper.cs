namespace Services.Helpers
{
    public static class TagHelper
    {
        /// <summary>
        /// Adds a game tag to the specified dictionary by parsing a line of text. For example, given the line
        /// [Event "CAN-op"], the method will add an entry to the dictionary with the key "event" and the value "CAN-op".
        /// </summary>
        /// <remarks>The tag name is converted to lowercase using the current culture. The square brackets
        /// around the tag name and any quotation marks in the tag value are removed during parsing.</remarks>
        /// <param name="tagDictionary">The dictionary to which the parsed tag will be added. The key represents the tag name, and the value
        /// represents the tag value.</param>
        /// <param name="line">A string containing the tag in the format "[TagName] TagValue". The tag name is enclosed in square brackets,
        /// and the tag value follows it.</param>
        public static void AddGameTag(Dictionary<string, string> tagDictionary, string line)
        {
            var tagSections = line.Split(" ", 2);

            string tagKey = tagSections[0].Trim('[').ToLower(System.Globalization.CultureInfo.CurrentCulture);

            string tagValue = tagSections[1].Trim(']').Replace("\"", "");

            tagDictionary[tagKey] = tagValue;
        }
    }
}
