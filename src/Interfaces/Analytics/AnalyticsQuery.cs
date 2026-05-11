namespace Interfaces.Analytics;

/// <summary>
/// Filter dimensions for analytics metric queries (PLAN §5.1). All filters combine with AND when set.
/// </summary>
public sealed class AnalyticsQuery
{
    public short? MinGameYear { get; init; }

    public short? MaxGameYear { get; init; }

    /// <summary>Exact match on White player's surname when set.</summary>
    public string? WhitePlayerSurname { get; init; }

    /// <summary>Exact match on White player's forenames when set. Empty string matches players with no forenames.</summary>
    public string? WhitePlayerForenames { get; init; }

    /// <summary>Exact match on Black player's surname when set.</summary>
    public string? BlackPlayerSurname { get; init; }

    /// <summary>Exact match on Black player's forenames when set. Empty string matches players with no forenames.</summary>
    public string? BlackPlayerForenames { get; init; }

    /// <summary>Exact match on <c>dbo.Game.Eco</c> when set.</summary>
    public string? Eco { get; init; }

    /// <summary>Primary player for player-comparison metrics.</summary>
    public string? PlayerASurname { get; init; }

    /// <summary>Primary player's forenames. Empty string matches players with no forenames.</summary>
    public string? PlayerAForenames { get; init; }

    /// <summary>Optional comparison player for player-comparison metrics. When omitted, metrics may compare against all players.</summary>
    public string? PlayerBSurname { get; init; }

    /// <summary>Comparison player's forenames. Empty string matches players with no forenames.</summary>
    public string? PlayerBForenames { get; init; }

    /// <summary>Colour mode for player-comparison metrics: <c>Any</c>, <c>White</c>, or <c>Black</c>.</summary>
    public string? PlayerColour { get; init; }

    /// <summary>User-facing full move number. For position summaries, full move N maps to ply <c>(N * 2) - 1</c>.</summary>
    public int? MoveNumber { get; init; }

    /// <summary>
    /// Board snapshot ply for average-material metric (must exist in <c>GamePositionSummary</c>).
    /// When null, the executor uses ply <c>4</c> (PLAN §5.3.4).
    /// </summary>
    public int? SummaryPlyIndex { get; init; }
}
