namespace Interfaces.Analytics;

/// <summary>
/// Fills <c>GameMove</c> / <c>GamePositionSummary</c> for games that already have <c>BoardPosition</c> rows but no move facts (PLAN §5.3.5).
/// </summary>
public interface IAnalyticsBackfillService
{
    /// <summary>
    /// For each game returned by <c>GetGameIdsNeedingAnalyticsBackfillAsync</c>, loads ordered board snapshots and runs the same materialization as ETL.
    /// </summary>
    Task<AnalyticsBackfillResult> BackfillMissingAnalyticsAsync(
        AnalyticsBackfillOptions? options = null,
        CancellationToken cancellationToken = default);
}
