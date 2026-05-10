namespace Interfaces.Analytics;

/// <summary>
/// Result of <see cref="IAnalyticsMaterializationService.MaterializeFromOrderedPliesAsync"/> for diagnostics and backfill accounting.
/// </summary>
public enum AnalyticsMaterializationOutcome
{
    /// <summary>Replacements ran without throwing.</summary>
    Success,

    /// <summary>No board snapshots were supplied.</summary>
    SkippedNoPositions,

    /// <summary>Non-contiguous plies or missing initial ply when half-moves exist (same rules as ETL path).</summary>
    SkippedInvalidSequence,

    /// <summary>An exception occurred before or during persistence (logged).</summary>
    Failed
}
