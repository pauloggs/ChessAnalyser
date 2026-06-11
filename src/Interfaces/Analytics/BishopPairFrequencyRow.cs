namespace Interfaces.Analytics;

public sealed class BishopPairFrequencyRow
{
    public string PlayerSurname { get; init; } = string.Empty;

    public string? PlayerForenames { get; init; }

    public int GameCount { get; init; }

    public double? AverageBishopPairFrequency { get; init; }

    public int? MinPlyIndex { get; init; }

    public int? MaxPlyIndex { get; init; }
}
