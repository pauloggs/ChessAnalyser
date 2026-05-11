namespace Interfaces.Analytics;

public sealed class PlayerMaterialAverageRow
{
    public string Series { get; init; } = string.Empty;

    public string? PlayerSurname { get; init; }

    public string? PlayerForenames { get; init; }

    public string Colour { get; init; } = string.Empty;

    public int MoveNumber { get; init; }

    public int PlyIndex { get; init; }

    public double AvgMaterial { get; init; }

    public int PositionCount { get; init; }
}
