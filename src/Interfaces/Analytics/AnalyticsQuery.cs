namespace Interfaces.Analytics;

/// <summary>
/// Filter dimensions for analytics metric queries (PLAN §5.1). All filters combine with AND when set.
/// </summary>
public sealed class AnalyticsQuery
{
    public short? MinGameYear { get; init; }

    public short? MaxGameYear { get; init; }

    public int? WhitePlayerId { get; init; }

    public int? BlackPlayerId { get; init; }

    /// <summary>Exact match on <c>dbo.Game.Eco</c> when set.</summary>
    public string? Eco { get; init; }

    /// <summary>
    /// Board snapshot ply for average-material metric (must exist in <c>GamePositionSummary</c>).
    /// When null, the executor uses ply <c>4</c> (PLAN §5.3.4).
    /// </summary>
    public int? SummaryPlyIndex { get; init; }
}
