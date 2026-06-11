namespace Interfaces.Analytics;

public sealed class MinorPieceCompositionRow
{
    public string PlayerSurname { get; init; } = string.Empty;

    public string? PlayerForenames { get; init; }

    public int GameCount { get; init; }

    public double? AverageMinorPieceDelta { get; init; }

    public int? MinPlyIndex { get; init; }

    public int? MaxPlyIndex { get; init; }
}
