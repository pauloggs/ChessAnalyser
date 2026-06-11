namespace Interfaces.Analytics;

public sealed class QueenTradeRateRow
{
    public string PlayerSurname { get; init; } = string.Empty;

    public string? PlayerForenames { get; init; }

    public int GameCount { get; init; }

    public double? QueenTradeRate { get; init; }

    public int QueenTradeMaxPly { get; init; }
}
