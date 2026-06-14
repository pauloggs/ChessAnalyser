using Interfaces.DTO.Ref;

namespace Services.PlayerMetadata;

/// <summary>
/// Matches PGN names to <c>Ref.FidePlayer</c> by surname + <see cref="PlayerForenamesMatcher"/>,
/// with birth-year filtering and title-based disambiguation when multiple candidates remain.
/// </summary>
public sealed class FidePlayerMatcher : IFidePlayerMatcher
{
    private static readonly HashSet<string> StrongTitles = new(StringComparer.OrdinalIgnoreCase)
    {
        "GM", "WGM", "IM", "WIM", "FM", "WFM"
    };

    /// <inheritdoc />
    public int? Match(
        string surname,
        string forenames,
        IReadOnlyList<FidePlayer> candidates,
        short? referenceGameYear)
    {
        if (string.IsNullOrWhiteSpace(surname) || candidates.Count == 0)
            return null;

        var surnameNorm = surname.Trim();
        var surnameMatches = candidates
            .Where(c => string.Equals(c.Surname, surnameNorm, StringComparison.OrdinalIgnoreCase))
            .ToList();
        if (surnameMatches.Count == 0)
            return null;

        PlayerForenameVariantHelper.TryGetCanonicalForenames(surnameNorm, forenames, out var canonicalForenames);

        var forenameMatches = surnameMatches
            .Where(c => ForenamesMatchForCatalog(forenames, canonicalForenames, c.Forenames))
            .ToList();
        if (forenameMatches.Count == 0)
            return null;

        var viable = forenameMatches
            .Where(c => IsBirthYearCompatible(c.BirthYear, referenceGameYear))
            .ToList();

        return viable.Count switch
        {
            0 => null,
            1 => viable[0].Id,
            _ => Disambiguate(forenames, canonicalForenames, viable, referenceGameYear)
        };
    }

    /// <summary>
    /// True when a candidate could have played in <paramref name="referenceGameYear"/>.
    /// </summary>
    public static bool IsBirthYearCompatible(short? birthYear, short? referenceGameYear) =>
        birthYear is null || referenceGameYear is null || birthYear <= referenceGameYear;

    private static bool ForenamesMatchForCatalog(
        string pgnForenames,
        string canonicalForenames,
        string? catalogForenames)
    {
        if (PlayerNameMatchHelper.ForenamesMatch(pgnForenames, catalogForenames))
            return true;

        return !string.IsNullOrEmpty(canonicalForenames)
            && PlayerNameMatchHelper.ForenamesMatch(canonicalForenames, catalogForenames);
    }

    private static int? Disambiguate(
        string pgnForenames,
        string canonicalForenames,
        IReadOnlyList<FidePlayer> viable,
        short? referenceGameYear)
    {
        var pgnNorm = PlayerNameMatchHelper.NormalizeForenames(pgnForenames);
        var isSingleLetterPgn = pgnNorm.Length == 1;

        var scored = viable
            .Select(c => (Player: c, Score: ScoreCandidate(pgnForenames, canonicalForenames, c, referenceGameYear, isSingleLetterPgn)))
            .OrderByDescending(x => x.Score)
            .ToList();

        if (scored[0].Score < 0)
            return null;

        if (scored.Count == 1 || scored[0].Score > scored[1].Score)
            return scored[0].Player.Id;

        return null;
    }

    private static int ScoreCandidate(
        string pgnForenames,
        string canonicalForenames,
        FidePlayer candidate,
        short? referenceGameYear,
        bool isSingleLetterPgn)
    {
        var score = 0;

        if (IsExactForenameMatch(pgnForenames, candidate.Forenames))
            score += 100;
        else if (!string.IsNullOrEmpty(canonicalForenames)
            && IsExactForenameMatch(canonicalForenames, candidate.Forenames))
            score += 90;
        else if (PlayerNameMatchHelper.ForenamesMatch(pgnForenames, candidate.Forenames))
            score += 60;
        else if (!string.IsNullOrEmpty(canonicalForenames)
            && PlayerNameMatchHelper.ForenamesMatch(canonicalForenames, candidate.Forenames))
            score += 50;

        if (!string.IsNullOrWhiteSpace(candidate.FideTitle) && StrongTitles.Contains(candidate.FideTitle))
            score += 40;

        if (referenceGameYear.HasValue && candidate.BirthYear.HasValue)
        {
            var age = referenceGameYear.Value - candidate.BirthYear.Value;
            if (age is >= 8 and <= 90)
                score += 10;
        }

        if (isSingleLetterPgn && score < 100)
            score -= 50;

        return score;
    }

    private static bool IsExactForenameMatch(string? forenames1, string? forenames2) =>
        string.Equals(
            PlayerNameMatchHelper.NormalizeForenames(forenames1),
            PlayerNameMatchHelper.NormalizeForenames(forenames2),
            StringComparison.OrdinalIgnoreCase);
}
