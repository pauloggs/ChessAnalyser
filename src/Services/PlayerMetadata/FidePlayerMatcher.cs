using Interfaces.DTO.Ref;
using Services.Helpers;

namespace Services.PlayerMetadata;

/// <summary>
/// Matches PGN names to <c>Ref.FidePlayer</c> by surname + <see cref="PlayerForenamesMatcher"/>,
/// with birth-year disambiguation when multiple candidates remain.
/// </summary>
public sealed class FidePlayerMatcher : IFidePlayerMatcher
{
    /// <inheritdoc />
    public int? Match(
        string surname,
        string forenames,
        IReadOnlyList<FidePlayer> candidates,
        short? referenceGameYear)
    {
        if (string.IsNullOrWhiteSpace(surname) || candidates.Count == 0)
            return null;

        var surnameMatches = candidates
            .Where(c => string.Equals(c.Surname, surname.Trim(), StringComparison.OrdinalIgnoreCase))
            .ToList();
        if (surnameMatches.Count == 0)
            return null;

        var forenameMatches = surnameMatches
            .Where(c => PlayerNameMatchHelper.ForenamesMatch(forenames, c.Forenames))
            .ToList();
        if (forenameMatches.Count == 0)
            return null;

        var viable = forenameMatches
            .Where(c => IsBirthYearCompatible(c.BirthYear, referenceGameYear))
            .ToList();

        return viable.Count == 1 ? viable[0].Id : null;
    }

    /// <summary>
    /// True when a candidate could have played in <paramref name="referenceGameYear"/>.
    /// </summary>
    public static bool IsBirthYearCompatible(short? birthYear, short? referenceGameYear) =>
        birthYear is null || referenceGameYear is null || birthYear <= referenceGameYear;
}
