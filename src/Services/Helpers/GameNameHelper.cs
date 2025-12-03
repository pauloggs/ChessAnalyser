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
                    tagList.Add(tagDicionary[tag]);
                }
            }

            return string.Join("|", tagList);
        }
    }
}
