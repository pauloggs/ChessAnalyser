using Interfaces.DTO;

namespace Services.PlayerMetadata;

/// <summary>
/// Loads official FIDE rating list files into <c>Ref.FidePlayer</c> and backfills corpus players.
/// </summary>
public interface IFideCatalogImportService
{
    /// <summary>
    /// Parses a FIDE TXT list, replaces <c>Ref.FidePlayer</c>, then backfills FIDE metadata on <see cref="Player"/> rows.
    /// </summary>
    Task<FideCatalogImportResult> ImportAndBackfillAsync(
        string fideListPath,
        bool dryRun = false,
        CancellationToken cancellationToken = default);
}
