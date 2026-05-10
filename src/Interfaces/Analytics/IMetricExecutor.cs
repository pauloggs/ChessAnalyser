namespace Interfaces.Analytics;

/// <summary>
/// Runs a single registered analytics metric (PLAN §5.3.4).
/// </summary>
public interface IMetricExecutor
{
    /// <summary>Stable key used by <see cref="IMetricRegistry"/> (e.g. <c>AverageMaterialByYearAndColour</c>).</summary>
    string MetricKey { get; }

    Task<AnalyticsTableResult> ExecuteAsync(AnalyticsQuery query, CancellationToken cancellationToken = default);
}
