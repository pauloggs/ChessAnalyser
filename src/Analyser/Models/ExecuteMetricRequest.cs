using Interfaces.Analytics;

namespace Analyser.Models;

/// <summary>
/// POST body for <c>POST /api/analytics/metrics/execute</c> (PLAN §12).
/// </summary>
public sealed class ExecuteMetricRequest
{
    /// <summary>Registered metric key (case-insensitive match to <see cref="IMetricRegistry"/>).</summary>
    public required string MetricKey { get; init; }

    /// <summary>Optional filters; omitted properties are treated as unset.</summary>
    public AnalyticsQuery? Query { get; init; }
}
