namespace Interfaces.Analytics;

/// <summary>
/// Per-player aggregate for corpus benchmark distributions.
/// </summary>
public sealed class PlayerStylePerPlayerMetricRow
{
    public string PlayerSurname { get; init; } = string.Empty;

    public string? PlayerForenames { get; init; }

    public int GameCount { get; init; }

    public double? MetricValue { get; init; }
}
