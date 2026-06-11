namespace Services.PlayerMetadata;

/// <summary>Maps FIDE title tokens from rating-list TXT to normalised codes stored on <see cref="Interfaces.DTO.Player"/>.</summary>
internal static class FideTitleNormalizer
{
    private static readonly Dictionary<string, string> SingleLetterCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["g"] = "GM",
        ["m"] = "IM",
        ["f"] = "FM",
        ["c"] = "CM",
        ["wg"] = "WGM",
        ["wm"] = "WIM",
        ["wf"] = "WFM",
        ["wc"] = "WCM",
    };

    private static readonly string[] MultiCharTitles = ["WGM", "WIM", "WFM", "WCM", "GM", "IM", "FM", "CM"];

    internal static string? Normalize(string? titleRegion)
    {
        if (string.IsNullOrWhiteSpace(titleRegion))
            return null;

        var compact = string.Concat(titleRegion.Where(c => !char.IsWhiteSpace(c)));
        if (compact.Length == 0)
            return null;

        foreach (var title in MultiCharTitles)
        {
            if (compact.Contains(title, StringComparison.OrdinalIgnoreCase))
                return title;
        }

        foreach (var pair in SingleLetterCodes)
        {
            if (compact.Equals(pair.Key, StringComparison.OrdinalIgnoreCase))
                return pair.Value;
        }

        return null;
    }
}
