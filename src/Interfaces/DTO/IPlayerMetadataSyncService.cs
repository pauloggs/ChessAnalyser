namespace Interfaces.DTO;

/// <summary>
/// Applies curated metadata (world champion, etc.) to persisted player rows.
/// </summary>
public interface IPlayerMetadataSyncService
{
    /// <summary>
    /// Recomputes <see cref="Player.WasWorldChampion"/> for every player from <c>Ref.WorldChampion</c>.
    /// </summary>
    Task<PlayerMetadataSyncResult> SyncWorldChampionFlagsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Matches corpus players against a FIDE rating list file and updates FIDE metadata columns.
    /// Does not modify <see cref="Player.WasWorldChampion"/>.
    /// </summary>
    Task<FideMetadataSyncResult> SyncFideMetadataAsync(
        string fideListPath,
        bool dryRun = false,
        CancellationToken cancellationToken = default);
}
