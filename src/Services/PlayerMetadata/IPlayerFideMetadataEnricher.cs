using Interfaces.DTO;

namespace Services.PlayerMetadata;

/// <summary>
/// Idempotent player metadata enrichment from <c>Ref.*</c> catalogs and corpus game years.
/// </summary>
public interface IPlayerFideMetadataEnricher
{
    /// <summary>
    /// Enriches one player (world champion flag + FIDE metadata). Safe to call repeatedly.
    /// </summary>
    Task<bool> TryEnrichPlayerAsync(
        int playerId,
        short? observedGameYear = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Enriches one player when surname/forenames are already known (e.g. before first game is persisted).
    /// </summary>
    Task<bool> TryEnrichPlayerAsync(
        int playerId,
        string? surname,
        string? forenames,
        short? observedGameYear = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Idempotent pass over all players: world-champion flags and FIDE metadata from Ref catalogs.
    /// </summary>
    Task<FideMetadataSyncResult> EnrichAllAsync(
        bool dryRun = false,
        CancellationToken cancellationToken = default);
}
