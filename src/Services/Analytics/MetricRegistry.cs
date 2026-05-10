using Interfaces.Analytics;

namespace Services.Analytics;

public sealed class MetricRegistry(IEnumerable<IMetricExecutor> executors) : IMetricRegistry
{
    private readonly IReadOnlyDictionary<string, IMetricExecutor> _executors =
        executors.ToDictionary(e => e.MetricKey, StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<string> MetricKeys => _executors.Keys.ToList();

    public IMetricExecutor? TryGetExecutor(string metricKey) =>
        _executors.TryGetValue(metricKey, out var e) ? e : null;

    public Task<AnalyticsTableResult> ExecuteAsync(string metricKey, AnalyticsQuery query, CancellationToken cancellationToken = default)
    {
        if (!_executors.TryGetValue(metricKey, out var executor))
            throw new KeyNotFoundException($"Unknown metric key: {metricKey}");
        return executor.ExecuteAsync(query, cancellationToken);
    }
}
