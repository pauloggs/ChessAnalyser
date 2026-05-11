namespace Analyser.Models;

/// <summary>
/// Static blurbs for Swagger and discovery; keys must stay aligned with metric executor classes in the Services project.
/// </summary>
internal static class MetricCatalog
{
    public static string? TryGetDescription(string metricKey)
    {
        if (string.IsNullOrEmpty(metricKey))
            return null;

        return metricKey.Trim() switch
        {
            "AverageMaterialByYearAndColour" =>
                "Corpus trend: average White-side and Black-side material at a fixed ply, grouped by GameYear. Not an independent player comparison metric.",
            "KnightMoveDestinationFrequency" =>
                "Count of knight half-moves grouped by destination square (ToSquare), with optional Game filters.",
            "GameCountByEco" =>
                "Count of games grouped by ECO code, with optional year, player-name, and ECO filters.",
            "AverageMaterialByPlayerAtMove" =>
                "Average material at a full move for Player A compared with Player B, or all players, with colour mode Any/White/Black.",
            _ => null
        };
    }
}
