using Services.Helpers;

namespace Services.PlayerMetadata;

/// <summary>
/// Matches player names against rows loaded from <c>Ref.WorldChampion</c>.
/// </summary>
public interface IWorldChampionMatcher
{
    /// <summary>Loads champion rows from the database when not already cached for this scope.</summary>
    Task EnsureLoadedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// True when <paramref name="surname"/> and <paramref name="forenames"/> match a reference row
    /// using <see cref="PlayerForenamesMatcher"/>.
    /// </summary>
    bool IsWorldChampion(string surname, string? forenames);
}
