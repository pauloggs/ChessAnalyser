namespace Interfaces.Analytics;

public sealed class FirstQueenMovePlyRow
{
    public string PlayerSurname { get; init; } = string.Empty;

    public string? PlayerForenames { get; init; }

    public int GamesWithQueenMove { get; init; }

    public double? AverageFirstQueenMovePly { get; init; }
}
