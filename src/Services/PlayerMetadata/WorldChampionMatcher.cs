using Interfaces.DTO.Ref;
using Services.Helpers;

namespace Services.PlayerMetadata;

/// <summary>
/// Matches PGN player names to <c>Ref.WorldChampion</c> using forename heuristics and known PGN variants.
/// </summary>
public sealed class WorldChampionMatcher : IWorldChampionMatcher
{
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

            if (PlayerForenameVariantHelper.TryGetCanonicalForenames(surnameNorm, forenames, out var variant)
                && PlayerNameMatchHelper.ForenamesMatch(variant, champion.Forenames))
                return champion.Id;
        }

        return null;
    }
}
