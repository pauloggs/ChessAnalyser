using Interfaces.DTO;

namespace Services.PlayerMetadata;

/// <summary>
/// Applies FIDE metadata from <c>Ref.FidePlayer</c> onto <see cref="Player"/> rows.
/// </summary>
public interface IPlayerFideMetadataEnricher
{
    /// <summary>
    /// Matches one player against the loaded FIDE catalog and updates FIDE columns when confident.
    /// </summary>
    Task<bool> TryEnrichPlayerAsync(
        int playerId,
        string surname,
        string forenames,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Recomputes FIDE metadata for every corpus player from <c>Ref.FidePlayer</c>.
    /// </summary>
    Task<FideMetadataSyncResult> BackfillAllAsync(
        bool dryRun = false,
        CancellationToken cancellationToken = default);
}
