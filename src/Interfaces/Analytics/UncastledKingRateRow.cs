namespace Interfaces.Analytics;

public sealed class UncastledKingRateRow
{
    public string PlayerSurname { get; init; } = string.Empty;

    public string? PlayerForenames { get; init; }

    public int GameCount { get; init; }

    public double? UncastledKingRate { get; init; }
}
