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
                "Player result summary from each player's perspective: wins, losses, draws, unknown results, total games, score, and score percentage.",
            "AverageMaterialByPlayerAtMove" =>
                "Average material at a full move for Player A compared with Player B, or all players, with colour mode Any/White/Black.",
            "AverageCastlingPly" =>
                "When does this player typically castle? Lower values mean an earlier first castle; higher values mean they delay castling more often. Games without castling are excluded from the average.",
            "AverageMaterialVolatility" =>
                "How much does the material balance swing during this player's games? Higher values suggest more dynamic, tactical play; lower values suggest steadier positions.",
            "BishopPairFrequency" =>
                "How often does this player keep both bishops during the middlegame? Higher values mean they retain the bishop pair in more of the sampled positions.",
            "MinorPieceComposition" =>
                "Is this player's minor-piece mix bishop-heavy or knight-heavy? Positive values favour bishops; negative values favour knights.",
            "CaptureRate" =>
                "How capture-happy is this player? Higher values mean a larger share of their moves are captures.",
            "QueenTradeRate" =>
                "How often do queens come off the board early in this player's games? Higher values mean queens are traded on or before the ply threshold more often.",
            "CentreMoveRate" =>
                "How often does this player play to the central squares? Higher values mean more moves land on d4, d5, e4, or e5.",
            "ForwardMoveRate" =>
                "How often does this player push into the opponent's half? Higher values suggest more aggressive, forward play.",
            "CastlingSidePreference" =>
                "Does this player prefer kingside or queenside castling? Among games where they castled, KingsideRate + QueensideRate = 1.",
            "OppositeSideCastlingRate" =>
                "How often do both players castle on opposite wings in this player's games? Higher values suggest more opposite-side castling structures.",
            "UncastledKingRate" =>
                "How often does this player leave the king uncastled? Higher values mean they more frequently play games without castling.",
            "FirstQueenMovePly" =>
                "How early does this player move their queen? Lower values mean an earlier first queen move. Games where the queen never moved are excluded.",
            "AverageGameLength" =>
                "How long are this player's games on average? Higher values mean longer games in half-moves; lower values mean shorter games.",
            "ShortDrawRate" =>
                "How often are this player's draws short? Higher values mean more of their drawn games end at or below the ply threshold (ChessBase fighting-spirit proxy).",
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
                "Wins and losses are calculated from each player's perspective; draws score 0.5 in Score.",
                "ScorePercentage = (wins + 0.5 × draws) ÷ (wins + losses + draws) × 100; unknown results are excluded from the denominator."
            ],
            "AverageMaterialByPlayerAtMove" =>
            [
                "Required: playerASurname (playerAForenames optional but recommended).",
                "Optional: playerBSurname/playerBForenames; omit Player B for all-player baseline.",
                "Optional: playerColour = Any, White, or Black (defaults to Any).",
                "Optional: moveNumber (defaults to 1; full move N maps to PlyIndex = N * 2 - 1).",
                "Optional: minGameYear, maxGameYear, eco."
            ],
            "AverageCastlingPly" =>
            [
                "How it's computed: half-move ply of the player's first castling move per game, then averaged.",
                "Required: playerSurname (playerForenames optional but recommended).",
                "Optional: playerColour = Any, White, or Black (defaults to Any).",
                "Optional: minGameYear, maxGameYear, eco.",
                "Games where the player never castled are excluded; see GamesWithCastling.",
                "Optional: includeCorpusBenchmark = true adds CorpusAverage, DeltaFromCorpus, CorpusPercentile, CorpusEligiblePlayerCount.",
                "Optional: benchmarkMinGames (default 30) for corpus eligibility."
            ],
            "AverageMaterialVolatility" =>
            [
                "How it's computed: per game, standard deviation of (your material − opponent's material) at each ply using classical piece values; then averaged across games.",
                "Required: playerSurname (playerForenames optional but recommended).",
                "Optional: playerColour = Any, White, or Black (defaults to Any).",
                "Optional: minGameYear, maxGameYear, eco.",
                "Optional: minPlyIndex, maxPlyIndex to limit which plies contribute (e.g. middlegame only).",
                "Optional: includeCorpusBenchmark = true adds CorpusAverage, DeltaFromCorpus, CorpusPercentile, CorpusEligiblePlayerCount.",
                "Optional: benchmarkMinGames (default 30) for corpus eligibility."
            ],
            "BishopPairFrequency" =>
            [
                "How it's computed: per game, share of plies in the window where the player has both bishops; then averaged across games.",
                "Required: playerSurname (playerForenames optional but recommended).",
                "Optional: playerColour = Any, White, or Black (defaults to Any).",
                "Optional: minGameYear, maxGameYear, eco.",
                "Optional: minPlyIndex, maxPlyIndex (defaults to 15 and 30 when both omitted).",
                "Optional: includeCorpusBenchmark = true adds CorpusAverage, DeltaFromCorpus, CorpusPercentile, CorpusEligiblePlayerCount.",
                "Optional: benchmarkMinGames (default 30) for corpus eligibility."
            ],
            "MinorPieceComposition" =>
            [
                "How it's computed: per ply in the window, bishops minus knights on the player's side; averaged per game, then across games.",
                "Required: playerSurname (playerForenames optional but recommended).",
                "Optional: playerColour = Any, White, or Black (defaults to Any).",
                "Optional: minGameYear, maxGameYear, eco.",
                "Optional: minPlyIndex, maxPlyIndex (defaults to 15 and 30 when both omitted).",
                "Optional: includeCorpusBenchmark = true adds CorpusAverage, DeltaFromCorpus, CorpusPercentile, CorpusEligiblePlayerCount.",
                "Optional: benchmarkMinGames (default 30) for corpus eligibility."
            ],
            "CaptureRate" =>
            [
                "How it's computed: per game, captures divided by total moves (capture = move with CapturedPiece set); then averaged across games.",
                "Required: playerSurname (playerForenames optional but recommended).",
                "Optional: playerColour = Any, White, or Black (defaults to Any).",
                "Optional: minGameYear, maxGameYear, eco.",
                "Optional: minPlyIndex, maxPlyIndex to restrict which move plies count.",
                "Optional: includeCorpusBenchmark = true adds CorpusAverage, DeltaFromCorpus, CorpusPercentile, CorpusEligiblePlayerCount.",
                "Optional: benchmarkMinGames (default 30) for corpus eligibility."
            ],
            "QueenTradeRate" =>
            [
                "How it's computed: per game, 1 if queens are not both on the board on or before queenTradeMaxPly, else 0; then averaged across games.",
                "Required: playerSurname (playerForenames optional but recommended).",
                "Optional: playerColour = Any, White, or Black (defaults to Any).",
                "Optional: minGameYear, maxGameYear, eco.",
                "Optional: queenTradeMaxPly (defaults to 40 when omitted).",
                "Optional: includeCorpusBenchmark = true adds CorpusAverage, DeltaFromCorpus, CorpusPercentile, CorpusEligiblePlayerCount.",
                "Optional: benchmarkMinGames (default 30) for corpus eligibility."
            ],
            "CentreMoveRate" =>
            [
                "How it's computed: per game, moves to d4/d5/e4/e5 divided by total moves; then averaged across games.",
                "Required: playerSurname (playerForenames optional but recommended).",
                "Optional: playerColour = Any, White, or Black (defaults to Any).",
                "Optional: minGameYear, maxGameYear, eco.",
                "Optional: minPlyIndex, maxPlyIndex to restrict which move plies count.",
                "Centre squares: d4, d5, e4, e5 (ToSquare 27, 28, 35, 36).",
                "Optional: includeCorpusBenchmark = true adds CorpusAverage, DeltaFromCorpus, CorpusPercentile, CorpusEligiblePlayerCount.",
                "Optional: benchmarkMinGames (default 30) for corpus eligibility."
            ],
            "ForwardMoveRate" =>
            [
                "How it's computed: per game, moves into the opponent's half divided by total moves; then averaged across games.",
                "Required: playerSurname (playerForenames optional but recommended).",
                "Optional: playerColour = Any, White, or Black (defaults to Any).",
                "Optional: minGameYear, maxGameYear, eco.",
                "Optional: minPlyIndex, maxPlyIndex to restrict which move plies count.",
                "Opponent's half: White ToSquare rank index ≥ 4; Black ToSquare rank index ≤ 3 (a1 = 0).",
                "Optional: includeCorpusBenchmark = true adds CorpusAverage, DeltaFromCorpus, CorpusPercentile, CorpusEligiblePlayerCount.",
                "Optional: benchmarkMinGames (default 30) for corpus eligibility."
            ],
            "CastlingSidePreference" =>
            [
                "How it's computed: first castling move per game only; kingside = 1 and queenside = 0, then averaged to KingsideRate (QueensideRate is the complement).",
                "Required: playerSurname (playerForenames optional but recommended).",
                "Optional: playerColour = Any, White, or Black (defaults to Any).",
                "Optional: minGameYear, maxGameYear, eco.",
                "Games without castling are excluded.",
                "Optional: includeCorpusBenchmark = true adds CorpusAverage, DeltaFromCorpus, CorpusPercentile, CorpusEligiblePlayerCount (benchmarks KingsideRate).",
                "Optional: benchmarkMinGames (default 30) for corpus eligibility."
            ],
            "OppositeSideCastlingRate" =>
            [
                "How it's computed: among games where both White and Black castled, 1 if they chose opposite wings (kingside vs queenside), else 0; then averaged.",
                "Required: playerSurname (playerForenames optional but recommended).",
                "Optional: playerColour = Any, White, or Black (defaults to Any).",
                "Optional: minGameYear, maxGameYear, eco.",
                "EligibleGameCount counts games where both sides castled; others are excluded.",
                "Optional: includeCorpusBenchmark = true adds CorpusAverage, DeltaFromCorpus, CorpusPercentile, CorpusEligiblePlayerCount.",
                "Optional: benchmarkMinGames (default 30) for corpus eligibility."
            ],
            "UncastledKingRate" =>
            [
                "How it's computed: per game, 1 if the player has no castling move, else 0; then averaged over all filtered games.",
                "Required: playerSurname (playerForenames optional but recommended).",
                "Optional: playerColour = Any, White, or Black (defaults to Any).",
                "Optional: minGameYear, maxGameYear, eco.",
                "GameCount is all filtered appearances.",
                "Optional: includeCorpusBenchmark = true adds CorpusAverage, DeltaFromCorpus, CorpusPercentile, CorpusEligiblePlayerCount.",
                "Optional: benchmarkMinGames (default 30) for corpus eligibility."
            ],
            "FirstQueenMovePly" =>
            [
                "How it's computed: half-move ply of the player's first queen move per game, then averaged (raw ply, not normalized by game length).",
                "Required: playerSurname (playerForenames optional but recommended).",
                "Optional: playerColour = Any, White, or Black (defaults to Any).",
                "Optional: minGameYear, maxGameYear, eco.",
                "Games where the queen never moved are excluded; see GamesWithQueenMove.",
                "Optional: includeCorpusBenchmark = true adds CorpusAverage, DeltaFromCorpus, CorpusPercentile, CorpusEligiblePlayerCount.",
                "Optional: benchmarkMinGames (default 30) for corpus eligibility."
            ],
            "AverageGameLength" =>
            [
                "How it's computed: per game, MAX(PlyIndex) from GamePositionSummary; then averaged across filtered appearances.",
                "Required: playerSurname (playerForenames optional but recommended).",
                "Optional: playerColour = Any, White, or Black (defaults to Any).",
                "Optional: minGameYear, maxGameYear, eco.",
                "Optional: includeCorpusBenchmark = true adds CorpusAverage, DeltaFromCorpus, CorpusPercentile, CorpusEligiblePlayerCount.",
                "Optional: benchmarkMinGames (default 30) for corpus eligibility."
            ],
            "ShortDrawRate" =>
            [
                "How it's computed: among draws only (Winner = D), 1 if MAX(PlyIndex) <= shortDrawMaxPly, else 0; then averaged.",
                "Required: playerSurname (playerForenames optional but recommended).",
                "Optional: playerColour = Any, White, or Black (defaults to Any).",
                "Optional: minGameYear, maxGameYear, eco.",
                "Optional: shortDrawMaxPly (defaults to 20 when omitted).",
                "DrawCount is the number of filtered draws; non-draws are excluded.",
                "Optional: includeCorpusBenchmark = true adds CorpusAverage, DeltaFromCorpus, CorpusPercentile, CorpusEligiblePlayerCount.",
                "Optional: benchmarkMinGames (default 30) for corpus eligibility (applied to draw count)."
            ],
            _ => []
        };
    }
}
