using Interfaces;

namespace Services.Helpers
{
    public static class GameNameHelper
    {
        public static string GetGameName(Dictionary<string, string> tagDicionary)
        {
            ArgumentNullException.ThrowIfNull(tagDicionary);

            if (tagDicionary.Count == 0)
            {
                throw new ArgumentException("Tag dictionary cannot be empty.", nameof(tagDicionary));
            }

            var tagList = new List<string>();

            foreach (var tag in Constants.GameTagIdentifiers)
            {
                if (tagDicionary.ContainsKey(tag))
                {
                    var value = tagDicionary[tag];
                    // Use dash when value is unknown or empty so the name is readable (e.g. "-|-|1904.??.??|-|White|Black|0-1|C33")
                    if (string.IsNullOrWhiteSpace(value) || value.Trim() == "?")
                        tagList.Add("-");
                    else
                        tagList.Add(value.Trim());
                }
            }

            return string.Join("|", tagList);
        }
    }
}
