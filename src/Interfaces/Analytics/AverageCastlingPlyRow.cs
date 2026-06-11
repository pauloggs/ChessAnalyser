namespace Interfaces.Analytics;

public sealed class AverageCastlingPlyRow
{
    public string PlayerSurname { get; init; } = string.Empty;

    public string? PlayerForenames { get; init; }

    public int GamesWithCastling { get; init; }

    public double? AverageCastlingPly { get; init; }
}
