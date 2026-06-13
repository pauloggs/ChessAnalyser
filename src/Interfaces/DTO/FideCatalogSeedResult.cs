namespace Interfaces.DTO;

/// <summary>Outcome of seeding <c>Ref.FidePlayer</c> from a FIDE rating-list file.</summary>
public sealed class FideCatalogSeedResult
{
    public bool Skipped { get; init; }

    public string? SkipReason { get; init; }

    public int RowsInserted { get; init; }

    public int RowsSkipped { get; init; }

    public string? SourcePath { get; init; }
}
