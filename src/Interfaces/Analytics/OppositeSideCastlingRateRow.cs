namespace Interfaces.Analytics;

public sealed class OppositeSideCastlingRateRow
{
    public string PlayerSurname { get; init; } = string.Empty;

    public string? PlayerForenames { get; init; }

    public int EligibleGameCount { get; init; }

    public double? OppositeSideCastlingRate { get; init; }
}
