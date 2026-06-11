namespace Interfaces.Analytics;

public sealed class CastlingSidePreferenceRow
{
    public string PlayerSurname { get; init; } = string.Empty;

    public string? PlayerForenames { get; init; }

    public int GamesWithCastling { get; init; }

    public double? KingsideRate { get; init; }

    public double? QueensideRate { get; init; }
}
