namespace Interfaces.Analytics;

public sealed class ForwardMoveRateRow
{
    public string PlayerSurname { get; init; } = string.Empty;

    public string? PlayerForenames { get; init; }

    public int GameCount { get; init; }

    public double? AverageForwardMoveRate { get; init; }
}
