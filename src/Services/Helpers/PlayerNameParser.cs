namespace Services.Helpers
{
    /// <summary>
    /// Parses PGN White/Black tag values into Surname and Forenames.
    /// With comma: "Giese, A" → Surname "Giese", Forenames "A".
    /// Without comma: "Alekhine Alexander A" → Surname "Alekhine", Forenames "Alexander A".
    /// All values are trimmed; forenames may be empty.
    /// </summary>
    public static class PlayerNameParser
    {
        /// <summary>
        /// Parse a single tag value (e.g. from [White "Giese, A"] or [Black "Alekhine Alexander A"]).
        /// </summary>
        /// <param name="tagValue">The value inside the quotes. May be null or empty.</param>
        /// <param name="surname">Surname (first word or part before first comma). Trimmed.</param>
        /// <param name="forenames">Forenames (rest after first comma, or after first word). Trimmed. Empty if none.</param>
        public static void Parse(string? tagValue, out string surname, out string forenames)
        {
            surname = string.Empty;
            forenames = string.Empty;
            var s = (tagValue ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(s))
                return;

            var commaIndex = s.IndexOf(',');
            if (commaIndex >= 0)
            {
                surname = s[..commaIndex].Trim();
                forenames = s[(commaIndex + 1)..].Trim();
                return;
            }

            var firstSpace = s.IndexOf(' ');
            if (firstSpace < 0)
            {
                surname = s;
                return;
            }
            surname = s[..firstSpace].Trim();
            forenames = s[(firstSpace + 1)..].Trim();
        }
    }
}
