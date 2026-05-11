namespace Analyser.Models;

/// <summary>
/// One entry from <c>GET /api/analytics/metrics</c> (discovery).
/// </summary>
public sealed class MetricDescriptorResponse
{
    public required string MetricKey { get; init; }

    /// <summary>Human-readable summary when known to the host catalog.</summary>
    public string? Description { get; init; }

    /// <summary>Short hints for the most relevant query parameters for this metric.</summary>
    public required IReadOnlyList<string> ParameterHints { get; init; }
}
