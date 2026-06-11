namespace Interfaces.Analytics;

public sealed class CentreMoveRateRow
{
    public string PlayerSurname { get; init; } = string.Empty;

    public string? PlayerForenames { get; init; }

    public int GameCount { get; init; }

    public double? AverageCentreMoveRate { get; init; }
}
