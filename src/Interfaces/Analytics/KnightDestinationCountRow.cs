namespace Interfaces.Analytics;

/// <summary>
/// Aggregated knight half-move count per destination square (0–63).
/// </summary>
public sealed class KnightDestinationCountRow
{
    public byte ToSquare { get; set; }

    public int MoveCount { get; set; }
}
