namespace Interfaces.Analytics;

/// <summary>
/// Summary counts from a backfill run.
/// </summary>
public sealed class AnalyticsBackfillResult
{
    public int GamesConsidered { get; init; }

    public int GamesMaterialized { get; init; }

    public int GamesSkipped { get; init; }

    public int GamesFailed { get; init; }
}
