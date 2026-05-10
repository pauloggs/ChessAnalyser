namespace Interfaces.Analytics;

/// <summary>
/// Resolves and runs <see cref="IMetricExecutor"/> instances registered at composition time (PLAN §5.3.4).
/// </summary>
public interface IMetricRegistry
{
    IReadOnlyCollection<string> MetricKeys { get; }

    IMetricExecutor? TryGetExecutor(string metricKey);

    Task<AnalyticsTableResult> ExecuteAsync(string metricKey, AnalyticsQuery query, CancellationToken cancellationToken = default);
}
