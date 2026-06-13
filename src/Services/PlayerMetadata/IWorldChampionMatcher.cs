using Interfaces.DTO.Ref;

namespace Services.PlayerMetadata;

/// <summary>
/// Matches PGN player names to <c>Ref.WorldChampion</c> rows (DESIGN §13.5).
/// </summary>
public interface IWorldChampionMatcher
{
    /// <summary>Returns the champion <see cref="WorldChampion.Id"/> when matched; otherwise null.</summary>
    int? Match(string surname, string forenames, IReadOnlyList<WorldChampion> catalog);
}
