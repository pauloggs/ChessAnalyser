using Interfaces.DTO;
using Repositories;

namespace Services.PlayerMetadata;

/// <inheritdoc />
public sealed class FideCatalogImportService(
    IChessRepository repository,
    IFideRatingListReader fideRatingListReader,
    IFidePlayerMatcher fidePlayerMatcher,
    IPlayerFideMetadataEnricher playerFideMetadataEnricher) : IFideCatalogImportService
{
    private readonly IChessRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly IFideRatingListReader _fideRatingListReader =
        fideRatingListReader ?? throw new ArgumentNullException(nameof(fideRatingListReader));
    private readonly IFidePlayerMatcher _fidePlayerMatcher =
        fidePlayerMatcher ?? throw new ArgumentNullException(nameof(fidePlayerMatcher));
    private readonly IPlayerFideMetadataEnricher _playerFideMetadataEnricher =
        playerFideMetadataEnricher ?? throw new ArgumentNullException(nameof(playerFideMetadataEnricher));

    /// <inheritdoc />
    public async Task<FideCatalogImportResult> ImportAndBackfillAsync(
        string fideListPath,
        bool dryRun = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fideListPath);

        var records = await _fideRatingListReader.ReadAsync(fideListPath, cancellationToken).ConfigureAwait(false);

        if (!dryRun)
        {
            await _repository.ReplaceFideCatalogAsync(records, cancellationToken).ConfigureAwait(false);
            _fidePlayerMatcher.InvalidateCache();
        }
        else
        {
            _fidePlayerMatcher.LoadCatalogSnapshot(records);
        }

        var backfill = await _playerFideMetadataEnricher.EnrichAllAsync(dryRun, cancellationToken)
            .ConfigureAwait(false);

        return new FideCatalogImportResult
        {
            CatalogRowsImported = dryRun ? 0 : records.Count,
            Backfill = backfill,
            DryRun = dryRun
        };
    }
}
