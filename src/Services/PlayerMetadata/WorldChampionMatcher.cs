using Interfaces.DTO.Ref;
using Services.Helpers;

namespace Services.PlayerMetadata;

/// <summary>
/// Matches PGN player names to <c>Ref.WorldChampion</c> using forename heuristics and known PGN variants.
/// </summary>
public sealed class WorldChampionMatcher : IWorldChampionMatcher
{
    /// <summary>
    /// Known PGN spellings mapped to canonical <c>Ref.WorldChampion.Forenames</c> (DESIGN §13.5 — C# only, no SQL alias table).
    /// </summary>
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

    /// <inheritdoc />
    public int? Match(string surname, string forenames, IReadOnlyList<WorldChampion> catalog)
    {
        if (string.IsNullOrWhiteSpace(surname) || catalog.Count == 0)
            return null;

        var surnameNorm = surname.Trim();
        foreach (var champion in catalog)
        {
            if (!string.Equals(champion.Surname, surnameNorm, StringComparison.OrdinalIgnoreCase))
                continue;

            if (PlayerNameMatchHelper.ForenamesMatch(forenames, champion.Forenames))
                return champion.Id;

            if (TryGetVariantForenames(surnameNorm, forenames, out var variant)
                && PlayerNameMatchHelper.ForenamesMatch(variant, champion.Forenames))
                return champion.Id;
        }

        return null;
    }

    private static bool TryGetVariantForenames(string surname, string forenames, out string variantForenames)
    {
        var normalizedForenames = PlayerNameMatchHelper.NormalizeForenames(forenames);
        foreach (var (variantSurname, pgnForenames, canonical) in ForenameVariants)
        {
            if (!string.Equals(variantSurname, surname, StringComparison.OrdinalIgnoreCase))
                continue;

            var variantKey = PlayerNameMatchHelper.NormalizeForenames(pgnForenames);
            if (VariantForenamesMatch(normalizedForenames, variantKey))
            {
                variantForenames = canonical;
                return true;
            }
        }

        variantForenames = string.Empty;
        return false;
    }

    /// <summary>
    /// True when PGN forenames equal the alias key, or the first forename token matches (e.g. Robert / Robert James).
    /// </summary>
    private static bool VariantForenamesMatch(string normalizedPgnForenames, string variantKey)
    {
        if (string.Equals(normalizedPgnForenames, variantKey, StringComparison.OrdinalIgnoreCase))
            return true;

        var firstToken = normalizedPgnForenames.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        return firstToken is not null
            && string.Equals(firstToken, variantKey, StringComparison.OrdinalIgnoreCase);
    }
}
