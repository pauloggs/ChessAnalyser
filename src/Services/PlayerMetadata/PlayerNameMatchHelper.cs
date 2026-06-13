using Services.Helpers;

namespace Services.PlayerMetadata;

internal static class PlayerNameMatchHelper
{
    /// <summary>
    /// Normalises forenames for catalog matching (slash prefix + trailing abbreviation dots).
    /// </summary>
    public static string NormalizeForenames(string? forenames)
    {
        var normalized = PlayerForenamesMatcher.NormalizeForMatching(forenames);
        var tokens = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.TrimEnd('.'))
            .Where(t => t.Length > 0);
        return string.Join(' ', tokens);
    }

    public static bool ForenamesMatch(string? forenames1, string? forenames2) =>
        PlayerForenamesMatcher.ForenamesMatch(
            NormalizeForenames(forenames1),
            NormalizeForenames(forenames2));
}
