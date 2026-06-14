namespace Services.PlayerMetadata;

/// <summary>
/// Known PGN forename spellings mapped to canonical catalog forenames (DESIGN §13.5 — C# only).
/// Shared by world-champion and FIDE matchers.
/// </summary>
internal static class PlayerForenameVariantHelper
{
    private static readonly (string Surname, string PgnForenames, string Canonical)[] ForenameVariants =
    [
        ("Fischer", "Robert", "Bobby"),
        ("Fischer", "Robert James", "Bobby"),
        ("Steinitz", "William", "Wilhelm"),
        ("Capablanca", "J", "Jose Raul"),
        ("Capablanca", "J.", "Jose Raul"),
        ("Alekhine", "A", "Alexander"),
        ("Alekhine", "A.", "Alexander"),
        ("Smyslov", "Vassily", "Vasily"),
        ("Kasparov", "Gary", "Garry"),
        ("Tal", "Mihail", "Mikhail"),
    ];

    /// <summary>
    /// Returns canonical forenames when <paramref name="forenames"/> matches a known PGN variant for the surname.
    /// </summary>
    public static bool TryGetCanonicalForenames(string surname, string forenames, out string canonicalForenames)
    {
        var normalizedForenames = PlayerNameMatchHelper.NormalizeForenames(forenames);
        foreach (var (variantSurname, pgnForenames, canonical) in ForenameVariants)
        {
            if (!string.Equals(variantSurname, surname.Trim(), StringComparison.OrdinalIgnoreCase))
                continue;

            var variantKey = PlayerNameMatchHelper.NormalizeForenames(pgnForenames);
            if (VariantForenamesMatch(normalizedForenames, variantKey))
            {
                canonicalForenames = canonical;
                return true;
            }
        }

        canonicalForenames = string.Empty;
        return false;
    }

    private static bool VariantForenamesMatch(string normalizedPgnForenames, string variantKey)
    {
        if (string.Equals(normalizedPgnForenames, variantKey, StringComparison.OrdinalIgnoreCase))
            return true;

        var firstToken = normalizedPgnForenames.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        return firstToken is not null
            && string.Equals(firstToken, variantKey, StringComparison.OrdinalIgnoreCase);
    }
}
