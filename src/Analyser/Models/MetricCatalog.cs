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
            "GameCountByYear" =>
                "Count of games grouped by parsed GameYear, with optional player-name and ECO filters.",
            "AverageMaterialByPlayerAtMove" =>
                "Average material at a full move for Player A compared with Player B, or all players, with colour mode Any/White/Black.",
            _ => null
        };
    }

    public static IReadOnlyList<string> GetParameterHints(string metricKey)
    {
        if (string.IsNullOrEmpty(metricKey))
            return [];

        return metricKey.Trim() switch
        {
            "AverageMaterialByYearAndColour" =>
            [
                "Optional: minGameYear, maxGameYear, eco.",
                "Optional: summaryPlyIndex (defaults to 4).",
                "Do not use for independent player-vs-player comparisons."
            ],
            "KnightMoveDestinationFrequency" =>
            [
                "Optional: minGameYear, maxGameYear, eco.",
                "Optional side filters: whitePlayerSurname/Forenames, blackPlayerSurname/Forenames.",
                "Returns numeric ToSquare values (0-63)."
            ],
            "GameCountByEco" =>
            [
                "Optional: minGameYear, maxGameYear, eco.",
                "Optional side filters: whitePlayerSurname/Forenames, blackPlayerSurname/Forenames.",
                "Rows with blank or missing ECO are excluded."
            ],
            "GameCountByYear" =>
            [
                "Optional: minGameYear, maxGameYear, eco.",
                "Optional side filters: whitePlayerSurname/Forenames, blackPlayerSurname/Forenames.",
                "Rows without a parsed GameYear are excluded."
            ],
            "AverageMaterialByPlayerAtMove" =>
            [
                "Required: playerASurname (playerAForenames optional but recommended).",
                "Optional: playerBSurname/playerBForenames; omit Player B for all-player baseline.",
                "Optional: playerColour = Any, White, or Black (defaults to Any).",
                "Optional: moveNumber (defaults to 1; full move N maps to PlyIndex = N * 2 - 1).",
                "Optional: minGameYear, maxGameYear, eco."
            ],
            _ => []
        };
    }
}
