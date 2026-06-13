using Interfaces.DTO;

namespace Services.PlayerMetadata;

/// <summary>
/// Links <c>App.Player</c> rows to <c>Ref.WorldChampion</c> and <c>Ref.FidePlayer</c> via matchers.
/// </summary>
public interface IPlayerMetadataLinkingService
{
    /// <summary>
    /// Attempts to set metadata FKs for one player. Idempotent; updates only when values change.
    /// </summary>
    Task<PlayerMetadataLinkResult> TryLinkPlayerAsync(
        int playerId,
        short? gameYear = null,
        CancellationToken cancellationToken = default);

    /// <summary>Backfills metadata FKs for every player in <c>App.Player</c>.</summary>
    Task<PlayerMetadataLinkResult> LinkAllPlayersAsync(
        IProgress<MaintenanceProgress>? progress = null,
        CancellationToken cancellationToken = default);
}
