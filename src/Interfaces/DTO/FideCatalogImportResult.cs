namespace Interfaces.DTO;

/// <summary>
/// Outcome of importing an official FIDE rating list into <c>Ref.FidePlayer</c> and backfilling <see cref="Player"/> rows.
/// </summary>
public sealed class FideCatalogImportResult
{
    public int CatalogRowsImported { get; init; }

    public FideMetadataSyncResult Backfill { get; init; } = new();

    public bool DryRun { get; init; }
}
