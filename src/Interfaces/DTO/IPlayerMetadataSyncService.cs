namespace Interfaces.DTO;

/// <summary>
/// Applies curated metadata (world champion, etc.) to persisted player rows.
/// </summary>
public interface IPlayerMetadataSyncService
{
    /// <summary>
    /// Recomputes <see cref="Player.WasWorldChampion"/> for every player from the in-app catalog.
    /// </summary>
    Task<PlayerMetadataSyncResult> SyncWorldChampionFlagsAsync(CancellationToken cancellationToken = default);
}
