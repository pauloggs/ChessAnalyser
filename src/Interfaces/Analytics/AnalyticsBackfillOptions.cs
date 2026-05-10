namespace Interfaces.Analytics;

/// <summary>
/// Options for <see cref="IAnalyticsBackfillService.BackfillMissingAnalyticsAsync"/>.
/// </summary>
public sealed class AnalyticsBackfillOptions
{
    /// <summary>
    /// Maximum number of games to process in this run (after ordering by game id). Null means no limit.
    /// </summary>
    public int? MaxGames { get; init; }
}
