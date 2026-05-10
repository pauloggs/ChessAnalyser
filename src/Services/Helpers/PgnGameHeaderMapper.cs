namespace Services.Helpers
{
    /// <summary>
    /// Maps PGN tag dictionary (lowercase keys, see <see cref="TagHelper"/>) onto explicit <see cref="Interfaces.DTO.Game"/>
    /// analytics columns before persistence (DESIGN §3.5, PLAN §5.1).
    /// </summary>
    public static class PgnGameHeaderMapper
    {
        /// <summary>
        /// Fills <see cref="Interfaces.DTO.Game"/> header fields from <see cref="Interfaces.DTO.Game.Tags"/>.
        /// Idempotent if called more than once with the same tags.
        /// </summary>
        public static void ApplyFromTags(Interfaces.DTO.Game game)
        {
            ArgumentNullException.ThrowIfNull(game);
            var tags = game.Tags ?? [];

            game.Event = NormalizeOptional(tags, "event");
            game.Site = NormalizeOptional(tags, "site");
            var dateRaw = NormalizeOptional(tags, "date");
            game.DateTag = dateRaw;
            game.GameYear = ParseGameYearFromPgnDate(dateRaw);
            game.Eco = TruncateOptional(tags, "eco", maxLen: 16);
        }

        private static string? NormalizeOptional(Dictionary<string, string> tags, string key)
        {
            if (!tags.TryGetValue(key, out var v))
                return null;
            v = v.Trim();
            return v.Length == 0 ? null : v;
        }

        private static string? TruncateOptional(Dictionary<string, string> tags, string key, int maxLen)
        {
            var s = NormalizeOptional(tags, key);
            if (s == null)
                return null;
            return s.Length <= maxLen ? s : s[..maxLen];
        }

        /// <summary>
        /// Year is set only when the PGN date's year segment is four digits with no <c>?</c> (DESIGN Q3; PLAN §9).
        /// </summary>
        public static short? ParseGameYearFromPgnDate(string? dateTag)
        {
            if (string.IsNullOrWhiteSpace(dateTag))
                return null;

            var parts = dateTag.Split('.');
            if (parts.Length < 2)
                return null;

            var yearPart = parts[0].Trim();
            if (yearPart.Length != 4)
                return null;
            if (yearPart.Contains('?', StringComparison.Ordinal))
                return null;
            if (!short.TryParse(yearPart, System.Globalization.NumberStyles.None,
                    System.Globalization.CultureInfo.InvariantCulture, out var y))
                return null;
            return y;
        }
    }
}
