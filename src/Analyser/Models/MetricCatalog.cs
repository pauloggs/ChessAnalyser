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
            "GameCountByResult" =>
                "Count of games grouped by result (White, Black, Draw, Unknown), with optional year, player-name, and ECO filters.",
            "GameCountByPlayer" =>
                "Count of player appearances, split by White and Black games, with optional year, player-name, and ECO filters.",
            "PlayerResultSummary" =>
                "Player result summary from each player's perspective: wins, losses, draws, unknown results, total games, and score.",
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
                "Optional: playerSurname/playerForenames plus playerColour = Any, White, or Black to narrow the game set.",
                "Optional: summaryPlyIndex (defaults to 4).",
                "Do not use for independent player-vs-player comparisons."
            ],
            "KnightMoveDestinationFrequency" =>
            [
                "Optional: minGameYear, maxGameYear, eco.",
                "Optional: playerSurname/playerForenames plus playerColour = Any, White, or Black.",
                "Returns numeric ToSquare values (0-63)."
            ],
            "GameCountByEco" =>
            [
                "Optional: minGameYear, maxGameYear, eco.",
                "Optional: playerSurname/playerForenames plus playerColour = Any, White, or Black.",
                "Rows with blank or missing ECO are excluded."
            ],
            "GameCountByYear" =>
            [
                "Optional: minGameYear, maxGameYear, eco.",
                "Optional: playerSurname/playerForenames plus playerColour = Any, White, or Black.",
                "Rows without a parsed GameYear are excluded."
            ],
            "GameCountByResult" =>
            [
                "Optional: minGameYear, maxGameYear, eco.",
                "Optional: playerSurname/playerForenames plus playerColour = Any, White, or Black.",
                "Stored winner codes are normalized to White, Black, Draw, or Unknown."
            ],
            "GameCountByPlayer" =>
            [
                "Optional: minGameYear, maxGameYear, eco.",
                "Optional player filter narrows the game set before counting player appearances.",
                "playerColour = Any, White, or Black is independent from player identity.",
                "Returns one row per resolved player with White, Black, and total game counts."
            ],
            "PlayerResultSummary" =>
            [
                "Optional: minGameYear, maxGameYear, eco.",
                "Optional player filter narrows the game set before summarizing player results.",
                "playerColour = Any, White, or Black is independent from player identity.",
                "Wins and losses are calculated from each player's perspective; draws score 0.5."
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
