namespace Repositories
{
	public class SqlStatements
	{
		public SqlStatements()
		{
		}

        public static string GetPlayers =>
            "SELECT Id, Surname, Forenames FROM dbo.Player;";

        public static string GetPlayerIdBySurnameAndForenames =>
            "SELECT Id FROM dbo.Player WHERE Surname = @Surname AND Forenames = @Forenames;";

        public static string GetPlayersBySurname =>
            "SELECT Id, Surname, Forenames FROM dbo.Player WHERE LOWER(Surname) = LOWER(@Surname);";

        public static string InsertPlayer =>
            """
            INSERT INTO dbo.Player (Surname, Forenames) VALUES (@Surname, @Forenames);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """;

        public static string InsertGame =>
        """
        IF (NOT EXISTS (SELECT TOP 1 Id FROM dbo.Game WHERE GameId = @GameId))
        BEGIN
            INSERT INTO dbo.Game (Name, GameId, Winner, WhitePlayerId, BlackPlayerId, Event, Site, DateTag, GameYear, Eco)
            VALUES (@Name, @GameId, @Winner, @WhitePlayerId, @BlackPlayerId, @Event, @Site, @DateTag, @GameYear, @Eco);
        END;
        SELECT Id FROM dbo.Game WHERE GameId = @GameId;
        """;

        public static string DeleteBoardPositionsForGame =>
            "DELETE FROM dbo.BoardPosition WHERE GameId = @GameId;";

        public static string InsertBoardPosition =>
        """
        INSERT INTO dbo.BoardPosition
        (GameId, PlyIndex, WP, WN, WB, WR, WQ, WK, BP, BN, BB, BR, BQ, BK, EnPassantTargetFile)
        VALUES
        (@GameId, @PlyIndex, @WP, @WN, @WB, @WR, @WQ, @WK, @BP, @BN, @BB, @BR, @BQ, @BK, @EnPassantTargetFile);
        """;

        public static string GetGameIds =>
        """
            SELECT 
        	    [GameId]
            FROM
        	    [Chess].[dbo].[Game];
        """;

        public static string InsertGameParseError =>
            """
            INSERT INTO dbo.GameParseError (SourcePgnFileName, GameIndexInFile, GameName, ErrorMessage)
            VALUES (@SourcePgnFileName, @GameIndexInFile, @GameName, @ErrorMessage);
            """;

        public static string DeleteGameMovesForGame =>
            "DELETE FROM dbo.GameMove WHERE GameId = @GameId;";

        public static string InsertGameMove =>
            """
            INSERT INTO dbo.GameMove
            (GameId, PlyIndex, MovingSide, FromSquare, ToSquare, MovedPiece, CapturedPiece, PromotionPiece, IsCastlingKingside, IsCastlingQueenside)
            VALUES (@GameId, @PlyIndex, @MovingSide, @FromSquare, @ToSquare, @MovedPiece, @CapturedPiece, @PromotionPiece, @IsCastlingKingside, @IsCastlingQueenside);
            """;

        public static string GetGameMovesForGame =>
            """
            SELECT GameId, PlyIndex, MovingSide, FromSquare, ToSquare, MovedPiece, CapturedPiece, PromotionPiece, IsCastlingKingside, IsCastlingQueenside
            FROM dbo.GameMove
            WHERE GameId = @GameId
            ORDER BY PlyIndex;
            """;

        public static string DeleteGamePositionSummariesForGame =>
            "DELETE FROM dbo.GamePositionSummary WHERE GameId = @GameId;";

        public static string InsertGamePositionSummary =>
            """
            INSERT INTO dbo.GamePositionSummary
            (GameId, PlyIndex, WhiteMaterial, BlackMaterial,
             WhitePawnCount, WhiteKnightCount, WhiteBishopCount, WhiteRookCount, WhiteQueenCount, WhiteKingCount,
             BlackPawnCount, BlackKnightCount, BlackBishopCount, BlackRookCount, BlackQueenCount, BlackKingCount)
            VALUES (@GameId, @PlyIndex, @WhiteMaterial, @BlackMaterial,
             @WhitePawnCount, @WhiteKnightCount, @WhiteBishopCount, @WhiteRookCount, @WhiteQueenCount, @WhiteKingCount,
             @BlackPawnCount, @BlackKnightCount, @BlackBishopCount, @BlackRookCount, @BlackQueenCount, @BlackKingCount);
            """;

        public static string GetGamePositionSummariesForGame =>
            """
            SELECT GameId, PlyIndex, WhiteMaterial, BlackMaterial,
             WhitePawnCount, WhiteKnightCount, WhiteBishopCount, WhiteRookCount, WhiteQueenCount, WhiteKingCount,
             BlackPawnCount, BlackKnightCount, BlackBishopCount, BlackRookCount, BlackQueenCount, BlackKingCount
            FROM dbo.GamePositionSummary
            WHERE GameId = @GameId
            ORDER BY PlyIndex;
            """;

        /// <summary>
        /// Year-based games only (<c>GameYear IS NOT NULL</c>); averages material at one ply (PLAN §5.3.4).
        /// </summary>
        public static string GetMaterialAveragesByYearAtPly =>
            """
            SELECT g.GameYear AS GameYear,
                   AVG(CAST(s.WhiteMaterial AS FLOAT)) AS AvgWhiteMaterial,
                   AVG(CAST(s.BlackMaterial AS FLOAT)) AS AvgBlackMaterial,
                   COUNT(*) AS GameCount
            FROM dbo.Game g
            INNER JOIN dbo.GamePositionSummary s ON s.GameId = g.Id AND s.PlyIndex = @PlyIndex
            LEFT JOIN dbo.Player wp ON wp.Id = g.WhitePlayerId
            LEFT JOIN dbo.Player bp ON bp.Id = g.BlackPlayerId
            WHERE g.GameYear IS NOT NULL
              AND (@MinGameYear IS NULL OR g.GameYear >= @MinGameYear)
              AND (@MaxGameYear IS NULL OR g.GameYear <= @MaxGameYear)
              AND (
                  @PlayerSurname IS NULL
                  OR ((@PlayerColour = 'Any' OR @PlayerColour = 'White') AND wp.Surname = @PlayerSurname AND (@PlayerForenames IS NULL OR wp.Forenames = @PlayerForenames))
                  OR ((@PlayerColour = 'Any' OR @PlayerColour = 'Black') AND bp.Surname = @PlayerSurname AND (@PlayerForenames IS NULL OR bp.Forenames = @PlayerForenames))
              )
              AND (@Eco IS NULL OR g.Eco = @Eco)
            GROUP BY g.GameYear
            ORDER BY g.GameYear;
            """;

        /// <summary>
        /// Knight half-moves grouped by destination square; optional filters on <c>Game</c> (PLAN §5.3.4).
        /// </summary>
        public static string GetKnightDestinationCounts =>
            """
            SELECT m.ToSquare AS ToSquare, COUNT(*) AS MoveCount
            FROM dbo.GameMove m
            INNER JOIN dbo.Game g ON g.Id = m.GameId
            LEFT JOIN dbo.Player wp ON wp.Id = g.WhitePlayerId
            LEFT JOIN dbo.Player bp ON bp.Id = g.BlackPlayerId
            WHERE m.MovedPiece = 'N'
              AND (@MinGameYear IS NULL OR (g.GameYear IS NOT NULL AND g.GameYear >= @MinGameYear))
              AND (@MaxGameYear IS NULL OR (g.GameYear IS NOT NULL AND g.GameYear <= @MaxGameYear))
              AND (
                  @PlayerSurname IS NULL
                  OR ((@PlayerColour = 'Any' OR @PlayerColour = 'White') AND wp.Surname = @PlayerSurname AND (@PlayerForenames IS NULL OR wp.Forenames = @PlayerForenames))
                  OR ((@PlayerColour = 'Any' OR @PlayerColour = 'Black') AND bp.Surname = @PlayerSurname AND (@PlayerForenames IS NULL OR bp.Forenames = @PlayerForenames))
              )
              AND (@Eco IS NULL OR g.Eco = @Eco)
            GROUP BY m.ToSquare
            ORDER BY MoveCount DESC, m.ToSquare;
            """;

        /// <summary>
        /// Games grouped by ECO code; optional filters on year, player names, and ECO.
        /// </summary>
        public static string GetGameCountsByEco =>
            """
            SELECT g.Eco AS Eco, COUNT(*) AS GameCount
            FROM dbo.Game g
            LEFT JOIN dbo.Player wp ON wp.Id = g.WhitePlayerId
            LEFT JOIN dbo.Player bp ON bp.Id = g.BlackPlayerId
            WHERE g.Eco IS NOT NULL
              AND LTRIM(RTRIM(g.Eco)) <> ''
              AND (@MinGameYear IS NULL OR (g.GameYear IS NOT NULL AND g.GameYear >= @MinGameYear))
              AND (@MaxGameYear IS NULL OR (g.GameYear IS NOT NULL AND g.GameYear <= @MaxGameYear))
              AND (
                  @PlayerSurname IS NULL
                  OR ((@PlayerColour = 'Any' OR @PlayerColour = 'White') AND wp.Surname = @PlayerSurname AND (@PlayerForenames IS NULL OR wp.Forenames = @PlayerForenames))
                  OR ((@PlayerColour = 'Any' OR @PlayerColour = 'Black') AND bp.Surname = @PlayerSurname AND (@PlayerForenames IS NULL OR bp.Forenames = @PlayerForenames))
              )
              AND (@Eco IS NULL OR g.Eco = @Eco)
            GROUP BY g.Eco
            ORDER BY GameCount DESC, g.Eco;
            """;

        /// <summary>
        /// Games grouped by calendar year; games without a parsed year are excluded.
        /// </summary>
        public static string GetGameCountsByYear =>
            """
            SELECT g.GameYear AS GameYear, COUNT(*) AS GameCount
            FROM dbo.Game g
            LEFT JOIN dbo.Player wp ON wp.Id = g.WhitePlayerId
            LEFT JOIN dbo.Player bp ON bp.Id = g.BlackPlayerId
            WHERE g.GameYear IS NOT NULL
              AND (@MinGameYear IS NULL OR g.GameYear >= @MinGameYear)
              AND (@MaxGameYear IS NULL OR g.GameYear <= @MaxGameYear)
              AND (
                  @PlayerSurname IS NULL
                  OR ((@PlayerColour = 'Any' OR @PlayerColour = 'White') AND wp.Surname = @PlayerSurname AND (@PlayerForenames IS NULL OR wp.Forenames = @PlayerForenames))
                  OR ((@PlayerColour = 'Any' OR @PlayerColour = 'Black') AND bp.Surname = @PlayerSurname AND (@PlayerForenames IS NULL OR bp.Forenames = @PlayerForenames))
              )
              AND (@Eco IS NULL OR g.Eco = @Eco)
            GROUP BY g.GameYear
            ORDER BY g.GameYear;
            """;

        /// <summary>
        /// Games grouped by normalized game result; optional filters on year, player names, and ECO.
        /// </summary>
        public static string GetGameCountsByResult =>
            """
            SELECT CASE g.Winner
                       WHEN 'W' THEN 'White'
                       WHEN 'B' THEN 'Black'
                       WHEN 'D' THEN 'Draw'
                       ELSE 'Unknown'
                   END AS Result,
                   COUNT(*) AS GameCount
            FROM dbo.Game g
            LEFT JOIN dbo.Player wp ON wp.Id = g.WhitePlayerId
            LEFT JOIN dbo.Player bp ON bp.Id = g.BlackPlayerId
            WHERE (@MinGameYear IS NULL OR (g.GameYear IS NOT NULL AND g.GameYear >= @MinGameYear))
              AND (@MaxGameYear IS NULL OR (g.GameYear IS NOT NULL AND g.GameYear <= @MaxGameYear))
              AND (
                  @PlayerSurname IS NULL
                  OR ((@PlayerColour = 'Any' OR @PlayerColour = 'White') AND wp.Surname = @PlayerSurname AND (@PlayerForenames IS NULL OR wp.Forenames = @PlayerForenames))
                  OR ((@PlayerColour = 'Any' OR @PlayerColour = 'Black') AND bp.Surname = @PlayerSurname AND (@PlayerForenames IS NULL OR bp.Forenames = @PlayerForenames))
              )
              AND (@Eco IS NULL OR g.Eco = @Eco)
            GROUP BY CASE g.Winner
                         WHEN 'W' THEN 'White'
                         WHEN 'B' THEN 'Black'
                         WHEN 'D' THEN 'Draw'
                         ELSE 'Unknown'
                     END
            ORDER BY GameCount DESC, Result;
            """;

        /// <summary>
        /// Player appearances grouped by player; optional filters narrow the game set first.
        /// </summary>
        public static string GetGameCountsByPlayer =>
            """
            WITH FilteredGames AS
            (
                SELECT g.Id,
                       g.WhitePlayerId,
                       g.BlackPlayerId
                FROM dbo.Game g
                LEFT JOIN dbo.Player wp ON wp.Id = g.WhitePlayerId
                LEFT JOIN dbo.Player bp ON bp.Id = g.BlackPlayerId
                WHERE (@MinGameYear IS NULL OR (g.GameYear IS NOT NULL AND g.GameYear >= @MinGameYear))
                  AND (@MaxGameYear IS NULL OR (g.GameYear IS NOT NULL AND g.GameYear <= @MaxGameYear))
                  AND (
                      @PlayerSurname IS NULL
                      OR ((@PlayerColour = 'Any' OR @PlayerColour = 'White') AND wp.Surname = @PlayerSurname AND (@PlayerForenames IS NULL OR wp.Forenames = @PlayerForenames))
                      OR ((@PlayerColour = 'Any' OR @PlayerColour = 'Black') AND bp.Surname = @PlayerSurname AND (@PlayerForenames IS NULL OR bp.Forenames = @PlayerForenames))
                  )
                  AND (@Eco IS NULL OR g.Eco = @Eco)
            ),
            Appearance AS
            (
                SELECT WhitePlayerId AS PlayerId,
                       1 AS WhiteGameCount,
                       0 AS BlackGameCount
                FROM FilteredGames
                WHERE WhitePlayerId IS NOT NULL

                UNION ALL

                SELECT BlackPlayerId AS PlayerId,
                       0 AS WhiteGameCount,
                       1 AS BlackGameCount
                FROM FilteredGames
                WHERE BlackPlayerId IS NOT NULL
            )
            SELECT p.Surname AS PlayerSurname,
                   p.Forenames AS PlayerForenames,
                   SUM(a.WhiteGameCount) AS WhiteGameCount,
                   SUM(a.BlackGameCount) AS BlackGameCount,
                   COUNT(*) AS TotalGameCount
            FROM Appearance a
            INNER JOIN dbo.Player p ON p.Id = a.PlayerId
            GROUP BY p.Surname, p.Forenames
            ORDER BY TotalGameCount DESC, p.Surname, p.Forenames;
            """;

        /// <summary>
        /// Player results from each player's perspective; optional filters narrow the game set first.
        /// </summary>
        public static string GetPlayerResultSummaries =>
            """
            WITH FilteredGames AS
            (
                SELECT g.Winner,
                       g.WhitePlayerId,
                       g.BlackPlayerId
                FROM dbo.Game g
                LEFT JOIN dbo.Player wp ON wp.Id = g.WhitePlayerId
                LEFT JOIN dbo.Player bp ON bp.Id = g.BlackPlayerId
                WHERE (@MinGameYear IS NULL OR (g.GameYear IS NOT NULL AND g.GameYear >= @MinGameYear))
                  AND (@MaxGameYear IS NULL OR (g.GameYear IS NOT NULL AND g.GameYear <= @MaxGameYear))
                  AND (
                      @PlayerSurname IS NULL
                      OR ((@PlayerColour = 'Any' OR @PlayerColour = 'White') AND wp.Surname = @PlayerSurname AND (@PlayerForenames IS NULL OR wp.Forenames = @PlayerForenames))
                      OR ((@PlayerColour = 'Any' OR @PlayerColour = 'Black') AND bp.Surname = @PlayerSurname AND (@PlayerForenames IS NULL OR bp.Forenames = @PlayerForenames))
                  )
                  AND (@Eco IS NULL OR g.Eco = @Eco)
            ),
            Appearance AS
            (
                SELECT WhitePlayerId AS PlayerId,
                       CASE WHEN Winner = 'W' THEN 1 ELSE 0 END AS WinCount,
                       CASE WHEN Winner = 'B' THEN 1 ELSE 0 END AS LossCount,
                       CASE WHEN Winner = 'D' THEN 1 ELSE 0 END AS DrawCount,
                       CASE WHEN Winner IS NULL OR Winner NOT IN ('W', 'B', 'D') THEN 1 ELSE 0 END AS UnknownCount
                FROM FilteredGames
                WHERE WhitePlayerId IS NOT NULL

                UNION ALL

                SELECT BlackPlayerId AS PlayerId,
                       CASE WHEN Winner = 'B' THEN 1 ELSE 0 END AS WinCount,
                       CASE WHEN Winner = 'W' THEN 1 ELSE 0 END AS LossCount,
                       CASE WHEN Winner = 'D' THEN 1 ELSE 0 END AS DrawCount,
                       CASE WHEN Winner IS NULL OR Winner NOT IN ('W', 'B', 'D') THEN 1 ELSE 0 END AS UnknownCount
                FROM FilteredGames
                WHERE BlackPlayerId IS NOT NULL
            )
            SELECT p.Surname AS PlayerSurname,
                   p.Forenames AS PlayerForenames,
                   SUM(a.WinCount) AS WinCount,
                   SUM(a.LossCount) AS LossCount,
                   SUM(a.DrawCount) AS DrawCount,
                   SUM(a.UnknownCount) AS UnknownCount,
                   COUNT(*) AS TotalGameCount,
                   CAST(SUM(a.WinCount) AS FLOAT) + (CAST(SUM(a.DrawCount) AS FLOAT) / 2.0) AS Score
            FROM Appearance a
            INNER JOIN dbo.Player p ON p.Id = a.PlayerId
            GROUP BY p.Surname, p.Forenames
            ORDER BY Score DESC, TotalGameCount DESC, p.Surname, p.Forenames;
            """;

        /// <summary>
        /// Player A average material at a ply compared with Player B, or all players when Player B is omitted.
        /// ColourMode is one of Any, White, Black.
        /// </summary>
        public static string GetPlayerMaterialAveragesAtPly =>
            """
            WITH Appearance AS
            (
                SELECT p.Surname,
                       p.Forenames,
                       CAST('White' AS NVARCHAR(8)) AS Colour,
                       s.WhiteMaterial AS Material
                FROM dbo.Game g
                INNER JOIN dbo.GamePositionSummary s ON s.GameId = g.Id AND s.PlyIndex = @PlyIndex
                INNER JOIN dbo.Player p ON p.Id = g.WhitePlayerId
                WHERE (@MinGameYear IS NULL OR (g.GameYear IS NOT NULL AND g.GameYear >= @MinGameYear))
                  AND (@MaxGameYear IS NULL OR (g.GameYear IS NOT NULL AND g.GameYear <= @MaxGameYear))
                  AND (@Eco IS NULL OR g.Eco = @Eco)

                UNION ALL

                SELECT p.Surname,
                       p.Forenames,
                       CAST('Black' AS NVARCHAR(8)) AS Colour,
                       s.BlackMaterial AS Material
                FROM dbo.Game g
                INNER JOIN dbo.GamePositionSummary s ON s.GameId = g.Id AND s.PlyIndex = @PlyIndex
                INNER JOIN dbo.Player p ON p.Id = g.BlackPlayerId
                WHERE (@MinGameYear IS NULL OR (g.GameYear IS NOT NULL AND g.GameYear >= @MinGameYear))
                  AND (@MaxGameYear IS NULL OR (g.GameYear IS NOT NULL AND g.GameYear <= @MaxGameYear))
                  AND (@Eco IS NULL OR g.Eco = @Eco)
            )
            SELECT Series,
                   PlayerSurname,
                   PlayerForenames,
                   Colour,
                   MoveNumber,
                   PlyIndex,
                   AvgMaterial,
                   PositionCount
            FROM
            (
                SELECT 1 AS SortOrder,
                       CAST('PlayerA' AS NVARCHAR(16)) AS Series,
                       @PlayerASurname AS PlayerSurname,
                       @PlayerAForenames AS PlayerForenames,
                       @ColourMode AS Colour,
                       @MoveNumber AS MoveNumber,
                       @PlyIndex AS PlyIndex,
                       AVG(CAST(Material AS FLOAT)) AS AvgMaterial,
                       COUNT(*) AS PositionCount
                FROM Appearance
                WHERE Surname = @PlayerASurname
                  AND (@PlayerAForenames IS NULL OR Forenames = @PlayerAForenames)
                  AND (@ColourMode = 'Any' OR Colour = @ColourMode)
                HAVING COUNT(*) > 0

                UNION ALL

                SELECT 2 AS SortOrder,
                       CASE WHEN @PlayerBSurname IS NULL THEN CAST('AllPlayers' AS NVARCHAR(16)) ELSE CAST('PlayerB' AS NVARCHAR(16)) END AS Series,
                       @PlayerBSurname AS PlayerSurname,
                       @PlayerBForenames AS PlayerForenames,
                       @ColourMode AS Colour,
                       @MoveNumber AS MoveNumber,
                       @PlyIndex AS PlyIndex,
                       AVG(CAST(Material AS FLOAT)) AS AvgMaterial,
                       COUNT(*) AS PositionCount
                FROM Appearance
                WHERE (@PlayerBSurname IS NULL OR (Surname = @PlayerBSurname AND (@PlayerBForenames IS NULL OR Forenames = @PlayerBForenames)))
                  AND (@ColourMode = 'Any' OR Colour = @ColourMode)
                HAVING COUNT(*) > 0
            ) x
            ORDER BY SortOrder;
            """;

        /// <summary>
        /// Mean first-castling ply for a filtered player; games without castling are excluded from the average.
        /// </summary>
        public static string GetAverageCastlingPly =>
            """
            WITH FilteredGames AS
            (
                SELECT g.Id AS GameId,
                       CASE
                           WHEN wp.Surname = @PlayerSurname AND (@PlayerForenames IS NULL OR wp.Forenames = @PlayerForenames) THEN CAST('W' AS CHAR(1))
                           WHEN bp.Surname = @PlayerSurname AND (@PlayerForenames IS NULL OR bp.Forenames = @PlayerForenames) THEN CAST('B' AS CHAR(1))
                           ELSE NULL
                       END AS PlayerSide
                FROM dbo.Game g
                INNER JOIN dbo.Player wp ON wp.Id = g.WhitePlayerId
                INNER JOIN dbo.Player bp ON bp.Id = g.BlackPlayerId
                WHERE (@MinGameYear IS NULL OR (g.GameYear IS NOT NULL AND g.GameYear >= @MinGameYear))
                  AND (@MaxGameYear IS NULL OR (g.GameYear IS NOT NULL AND g.GameYear <= @MaxGameYear))
                  AND (@Eco IS NULL OR g.Eco = @Eco)
                  AND (
                      (@PlayerColour = 'Any' AND (
                          (wp.Surname = @PlayerSurname AND (@PlayerForenames IS NULL OR wp.Forenames = @PlayerForenames))
                          OR (bp.Surname = @PlayerSurname AND (@PlayerForenames IS NULL OR bp.Forenames = @PlayerForenames))
                      ))
                      OR (@PlayerColour = 'White' AND wp.Surname = @PlayerSurname AND (@PlayerForenames IS NULL OR wp.Forenames = @PlayerForenames))
                      OR (@PlayerColour = 'Black' AND bp.Surname = @PlayerSurname AND (@PlayerForenames IS NULL OR bp.Forenames = @PlayerForenames))
                  )
            ),
            FirstCastle AS
            (
                SELECT fg.GameId, MIN(m.PlyIndex) AS CastlingPly
                FROM FilteredGames fg
                INNER JOIN dbo.GameMove m ON m.GameId = fg.GameId
                WHERE fg.PlayerSide IS NOT NULL
                  AND (m.IsCastlingKingside = 1 OR m.IsCastlingQueenside = 1)
                  AND m.MovingSide = fg.PlayerSide
                GROUP BY fg.GameId
            )
            SELECT @PlayerSurname AS PlayerSurname,
                   @PlayerForenames AS PlayerForenames,
                   COUNT(*) AS GamesWithCastling,
                   AVG(CAST(CastlingPly AS FLOAT)) AS AverageCastlingPly
            FROM FirstCastle;
            """;

        /// <summary>
        /// Mean per-game sample standard deviation of signed material balance from the player's perspective.
        /// </summary>
        public static string GetAverageMaterialVolatility =>
            """
            WITH FilteredGames AS
            (
                SELECT g.Id AS GameId,
                       CASE
                           WHEN wp.Surname = @PlayerSurname AND (@PlayerForenames IS NULL OR wp.Forenames = @PlayerForenames) THEN CAST('W' AS CHAR(1))
                           WHEN bp.Surname = @PlayerSurname AND (@PlayerForenames IS NULL OR bp.Forenames = @PlayerForenames) THEN CAST('B' AS CHAR(1))
                           ELSE NULL
                       END AS PlayerSide
                FROM dbo.Game g
                INNER JOIN dbo.Player wp ON wp.Id = g.WhitePlayerId
                INNER JOIN dbo.Player bp ON bp.Id = g.BlackPlayerId
                WHERE (@MinGameYear IS NULL OR (g.GameYear IS NOT NULL AND g.GameYear >= @MinGameYear))
                  AND (@MaxGameYear IS NULL OR (g.GameYear IS NOT NULL AND g.GameYear <= @MaxGameYear))
                  AND (@Eco IS NULL OR g.Eco = @Eco)
                  AND (
                      (@PlayerColour = 'Any' AND (
                          (wp.Surname = @PlayerSurname AND (@PlayerForenames IS NULL OR wp.Forenames = @PlayerForenames))
                          OR (bp.Surname = @PlayerSurname AND (@PlayerForenames IS NULL OR bp.Forenames = @PlayerForenames))
                      ))
                      OR (@PlayerColour = 'White' AND wp.Surname = @PlayerSurname AND (@PlayerForenames IS NULL OR wp.Forenames = @PlayerForenames))
                      OR (@PlayerColour = 'Black' AND bp.Surname = @PlayerSurname AND (@PlayerForenames IS NULL OR bp.Forenames = @PlayerForenames))
                  )
            ),
            PerPlyBalance AS
            (
                SELECT fg.GameId,
                       CASE fg.PlayerSide
                           WHEN 'W' THEN CAST(s.WhiteMaterial - s.BlackMaterial AS FLOAT)
                           WHEN 'B' THEN CAST(s.BlackMaterial - s.WhiteMaterial AS FLOAT)
                       END AS SignedBalance
                FROM FilteredGames fg
                INNER JOIN dbo.GamePositionSummary s ON s.GameId = fg.GameId
                WHERE fg.PlayerSide IS NOT NULL
                  AND (@MinPlyIndex IS NULL OR s.PlyIndex >= @MinPlyIndex)
                  AND (@MaxPlyIndex IS NULL OR s.PlyIndex <= @MaxPlyIndex)
            ),
            PerGameVolatility AS
            (
                SELECT GameId, STDEV(SignedBalance) AS MaterialVolatility
                FROM PerPlyBalance
                GROUP BY GameId
                HAVING COUNT(*) >= 2
            )
            SELECT @PlayerSurname AS PlayerSurname,
                   @PlayerForenames AS PlayerForenames,
                   COUNT(*) AS GameCount,
                   AVG(MaterialVolatility) AS AverageMaterialVolatility
            FROM PerGameVolatility;
            """;

        /// <summary>
        /// Per-player mean per-game material volatility for corpus benchmarks (PLAN §12.7).
        /// </summary>
        public static string GetPerPlayerAverageMaterialVolatility =>
            """
            WITH FilteredGames AS
            (
                SELECT g.Id AS GameId,
                       g.WhitePlayerId,
                       g.BlackPlayerId,
                       wp.Surname AS WhiteSurname,
                       wp.Forenames AS WhiteForenames,
                       bp.Surname AS BlackSurname,
                       bp.Forenames AS BlackForenames
                FROM dbo.Game g
                INNER JOIN dbo.Player wp ON wp.Id = g.WhitePlayerId
                INNER JOIN dbo.Player bp ON bp.Id = g.BlackPlayerId
                WHERE (@MinGameYear IS NULL OR (g.GameYear IS NOT NULL AND g.GameYear >= @MinGameYear))
                  AND (@MaxGameYear IS NULL OR (g.GameYear IS NOT NULL AND g.GameYear <= @MaxGameYear))
                  AND (@Eco IS NULL OR g.Eco = @Eco)
            ),
            Appearances AS
            (
                SELECT fg.GameId,
                       fg.WhiteSurname AS PlayerSurname,
                       fg.WhiteForenames AS PlayerForenames,
                       CAST('W' AS CHAR(1)) AS PlayerSide
                FROM FilteredGames fg
                WHERE @PlayerColour = 'Any' OR @PlayerColour = 'White'

                UNION ALL

                SELECT fg.GameId,
                       fg.BlackSurname,
                       fg.BlackForenames,
                       CAST('B' AS CHAR(1))
                FROM FilteredGames fg
                WHERE @PlayerColour = 'Any' OR @PlayerColour = 'Black'
            ),
            PerPlyBalance AS
            (
                SELECT a.PlayerSurname,
                       a.PlayerForenames,
                       a.GameId,
                       CASE a.PlayerSide
                           WHEN 'W' THEN CAST(s.WhiteMaterial - s.BlackMaterial AS FLOAT)
                           WHEN 'B' THEN CAST(s.BlackMaterial - s.WhiteMaterial AS FLOAT)
                       END AS SignedBalance
                FROM Appearances a
                INNER JOIN dbo.GamePositionSummary s ON s.GameId = a.GameId
                WHERE (@MinPlyIndex IS NULL OR s.PlyIndex >= @MinPlyIndex)
                  AND (@MaxPlyIndex IS NULL OR s.PlyIndex <= @MaxPlyIndex)
            ),
            PerGameVolatility AS
            (
                SELECT PlayerSurname,
                       PlayerForenames,
                       GameId,
                       STDEV(SignedBalance) AS MaterialVolatility
                FROM PerPlyBalance
                GROUP BY PlayerSurname, PlayerForenames, GameId
                HAVING COUNT(*) >= 2
            )
            SELECT PlayerSurname,
                   PlayerForenames,
                   COUNT(*) AS GameCount,
                   AVG(MaterialVolatility) AS MetricValue
            FROM PerGameVolatility
            GROUP BY PlayerSurname, PlayerForenames
            ORDER BY MetricValue DESC, PlayerSurname, PlayerForenames;
            """;

        /// <summary>
        /// Mean per-game share of plies in a window where the filtered player has both bishops.
        /// </summary>
        public static string GetBishopPairFrequency =>
            """
            WITH FilteredGames AS
            (
                SELECT g.Id AS GameId,
                       CASE
                           WHEN wp.Surname = @PlayerSurname AND (@PlayerForenames IS NULL OR wp.Forenames = @PlayerForenames) THEN CAST('W' AS CHAR(1))
                           WHEN bp.Surname = @PlayerSurname AND (@PlayerForenames IS NULL OR bp.Forenames = @PlayerForenames) THEN CAST('B' AS CHAR(1))
                           ELSE NULL
                       END AS PlayerSide
                FROM dbo.Game g
                INNER JOIN dbo.Player wp ON wp.Id = g.WhitePlayerId
                INNER JOIN dbo.Player bp ON bp.Id = g.BlackPlayerId
                WHERE (@MinGameYear IS NULL OR (g.GameYear IS NOT NULL AND g.GameYear >= @MinGameYear))
                  AND (@MaxGameYear IS NULL OR (g.GameYear IS NOT NULL AND g.GameYear <= @MaxGameYear))
                  AND (@Eco IS NULL OR g.Eco = @Eco)
                  AND (
                      (@PlayerColour = 'Any' AND (
                          (wp.Surname = @PlayerSurname AND (@PlayerForenames IS NULL OR wp.Forenames = @PlayerForenames))
                          OR (bp.Surname = @PlayerSurname AND (@PlayerForenames IS NULL OR bp.Forenames = @PlayerForenames))
                      ))
                      OR (@PlayerColour = 'White' AND wp.Surname = @PlayerSurname AND (@PlayerForenames IS NULL OR wp.Forenames = @PlayerForenames))
                      OR (@PlayerColour = 'Black' AND bp.Surname = @PlayerSurname AND (@PlayerForenames IS NULL OR bp.Forenames = @PlayerForenames))
                  )
            ),
            PerPly AS
            (
                SELECT fg.GameId,
                       CASE
                           WHEN fg.PlayerSide = 'W' AND s.WhiteBishopCount = 2 THEN 1.0
                           WHEN fg.PlayerSide = 'B' AND s.BlackBishopCount = 2 THEN 1.0
                           ELSE 0.0
                       END AS HasBishopPair
                FROM FilteredGames fg
                INNER JOIN dbo.GamePositionSummary s ON s.GameId = fg.GameId
                WHERE fg.PlayerSide IS NOT NULL
                  AND (@MinPlyIndex IS NULL OR s.PlyIndex >= @MinPlyIndex)
                  AND (@MaxPlyIndex IS NULL OR s.PlyIndex <= @MaxPlyIndex)
            ),
            PerGame AS
            (
                SELECT GameId, AVG(HasBishopPair) AS BishopPairFrequency
                FROM PerPly
                GROUP BY GameId
                HAVING COUNT(*) > 0
            )
            SELECT @PlayerSurname AS PlayerSurname,
                   @PlayerForenames AS PlayerForenames,
                   COUNT(*) AS GameCount,
                   AVG(BishopPairFrequency) AS AverageBishopPairFrequency,
                   @MinPlyIndex AS MinPlyIndex,
                   @MaxPlyIndex AS MaxPlyIndex
            FROM PerGame;
            """;

        /// <summary>
        /// Per-player mean bishop-pair frequency for corpus benchmarks (PLAN §12.7).
        /// </summary>
        public static string GetPerPlayerBishopPairFrequency =>
            """
            WITH FilteredGames AS
            (
                SELECT g.Id AS GameId,
                       g.WhitePlayerId,
                       g.BlackPlayerId,
                       wp.Surname AS WhiteSurname,
                       wp.Forenames AS WhiteForenames,
                       bp.Surname AS BlackSurname,
                       bp.Forenames AS BlackForenames
                FROM dbo.Game g
                INNER JOIN dbo.Player wp ON wp.Id = g.WhitePlayerId
                INNER JOIN dbo.Player bp ON bp.Id = g.BlackPlayerId
                WHERE (@MinGameYear IS NULL OR (g.GameYear IS NOT NULL AND g.GameYear >= @MinGameYear))
                  AND (@MaxGameYear IS NULL OR (g.GameYear IS NOT NULL AND g.GameYear <= @MaxGameYear))
                  AND (@Eco IS NULL OR g.Eco = @Eco)
            ),
            Appearances AS
            (
                SELECT fg.GameId,
                       fg.WhiteSurname AS PlayerSurname,
                       fg.WhiteForenames AS PlayerForenames,
                       CAST('W' AS CHAR(1)) AS PlayerSide
                FROM FilteredGames fg
                WHERE @PlayerColour = 'Any' OR @PlayerColour = 'White'

                UNION ALL

                SELECT fg.GameId,
                       fg.BlackSurname,
                       fg.BlackForenames,
                       CAST('B' AS CHAR(1))
                FROM FilteredGames fg
                WHERE @PlayerColour = 'Any' OR @PlayerColour = 'Black'
            ),
            PerPly AS
            (
                SELECT a.PlayerSurname,
                       a.PlayerForenames,
                       a.GameId,
                       CASE
                           WHEN a.PlayerSide = 'W' AND s.WhiteBishopCount = 2 THEN 1.0
                           WHEN a.PlayerSide = 'B' AND s.BlackBishopCount = 2 THEN 1.0
                           ELSE 0.0
                       END AS HasBishopPair
                FROM Appearances a
                INNER JOIN dbo.GamePositionSummary s ON s.GameId = a.GameId
                WHERE (@MinPlyIndex IS NULL OR s.PlyIndex >= @MinPlyIndex)
                  AND (@MaxPlyIndex IS NULL OR s.PlyIndex <= @MaxPlyIndex)
            ),
            PerGame AS
            (
                SELECT PlayerSurname,
                       PlayerForenames,
                       GameId,
                       AVG(HasBishopPair) AS BishopPairFrequency
                FROM PerPly
                GROUP BY PlayerSurname, PlayerForenames, GameId
                HAVING COUNT(*) > 0
            )
            SELECT PlayerSurname,
                   PlayerForenames,
                   COUNT(*) AS GameCount,
                   AVG(BishopPairFrequency) AS MetricValue
            FROM PerGame
            GROUP BY PlayerSurname, PlayerForenames
            ORDER BY MetricValue DESC, PlayerSurname, PlayerForenames;
            """;

        /// <summary>
        /// Mean per-game average of bishops minus knights on the filtered player's side in a ply window.
        /// </summary>
        public static string GetMinorPieceComposition =>
            """
            WITH FilteredGames AS
            (
                SELECT g.Id AS GameId,
                       CASE
                           WHEN wp.Surname = @PlayerSurname AND (@PlayerForenames IS NULL OR wp.Forenames = @PlayerForenames) THEN CAST('W' AS CHAR(1))
                           WHEN bp.Surname = @PlayerSurname AND (@PlayerForenames IS NULL OR bp.Forenames = @PlayerForenames) THEN CAST('B' AS CHAR(1))
                           ELSE NULL
                       END AS PlayerSide
                FROM dbo.Game g
                INNER JOIN dbo.Player wp ON wp.Id = g.WhitePlayerId
                INNER JOIN dbo.Player bp ON bp.Id = g.BlackPlayerId
                WHERE (@MinGameYear IS NULL OR (g.GameYear IS NOT NULL AND g.GameYear >= @MinGameYear))
                  AND (@MaxGameYear IS NULL OR (g.GameYear IS NOT NULL AND g.GameYear <= @MaxGameYear))
                  AND (@Eco IS NULL OR g.Eco = @Eco)
                  AND (
                      (@PlayerColour = 'Any' AND (
                          (wp.Surname = @PlayerSurname AND (@PlayerForenames IS NULL OR wp.Forenames = @PlayerForenames))
                          OR (bp.Surname = @PlayerSurname AND (@PlayerForenames IS NULL OR bp.Forenames = @PlayerForenames))
                      ))
                      OR (@PlayerColour = 'White' AND wp.Surname = @PlayerSurname AND (@PlayerForenames IS NULL OR wp.Forenames = @PlayerForenames))
                      OR (@PlayerColour = 'Black' AND bp.Surname = @PlayerSurname AND (@PlayerForenames IS NULL OR bp.Forenames = @PlayerForenames))
                  )
            ),
            PerPly AS
            (
                SELECT fg.GameId,
                       CASE fg.PlayerSide
                           WHEN 'W' THEN CAST(s.WhiteBishopCount - s.WhiteKnightCount AS FLOAT)
                           WHEN 'B' THEN CAST(s.BlackBishopCount - s.BlackKnightCount AS FLOAT)
                       END AS MinorPieceDelta
                FROM FilteredGames fg
                INNER JOIN dbo.GamePositionSummary s ON s.GameId = fg.GameId
                WHERE fg.PlayerSide IS NOT NULL
                  AND (@MinPlyIndex IS NULL OR s.PlyIndex >= @MinPlyIndex)
                  AND (@MaxPlyIndex IS NULL OR s.PlyIndex <= @MaxPlyIndex)
            ),
            PerGame AS
            (
                SELECT GameId, AVG(MinorPieceDelta) AS AvgMinorPieceDelta
                FROM PerPly
                GROUP BY GameId
                HAVING COUNT(*) > 0
            )
            SELECT @PlayerSurname AS PlayerSurname,
                   @PlayerForenames AS PlayerForenames,
                   COUNT(*) AS GameCount,
                   AVG(AvgMinorPieceDelta) AS AverageMinorPieceDelta,
                   @MinPlyIndex AS MinPlyIndex,
                   @MaxPlyIndex AS MaxPlyIndex
            FROM PerGame;
            """;

        /// <summary>
        /// Mean per-game capture rate for the filtered player's moves (capture ⇔ CapturedPiece IS NOT NULL).
        /// </summary>
        public static string GetCaptureRate =>
            """
            WITH FilteredGames AS
            (
                SELECT g.Id AS GameId,
                       CASE
                           WHEN wp.Surname = @PlayerSurname AND (@PlayerForenames IS NULL OR wp.Forenames = @PlayerForenames) THEN CAST('W' AS CHAR(1))
                           WHEN bp.Surname = @PlayerSurname AND (@PlayerForenames IS NULL OR bp.Forenames = @PlayerForenames) THEN CAST('B' AS CHAR(1))
                           ELSE NULL
                       END AS PlayerSide
                FROM dbo.Game g
                INNER JOIN dbo.Player wp ON wp.Id = g.WhitePlayerId
                INNER JOIN dbo.Player bp ON bp.Id = g.BlackPlayerId
                WHERE (@MinGameYear IS NULL OR (g.GameYear IS NOT NULL AND g.GameYear >= @MinGameYear))
                  AND (@MaxGameYear IS NULL OR (g.GameYear IS NOT NULL AND g.GameYear <= @MaxGameYear))
                  AND (@Eco IS NULL OR g.Eco = @Eco)
                  AND (
                      (@PlayerColour = 'Any' AND (
                          (wp.Surname = @PlayerSurname AND (@PlayerForenames IS NULL OR wp.Forenames = @PlayerForenames))
                          OR (bp.Surname = @PlayerSurname AND (@PlayerForenames IS NULL OR bp.Forenames = @PlayerForenames))
                      ))
                      OR (@PlayerColour = 'White' AND wp.Surname = @PlayerSurname AND (@PlayerForenames IS NULL OR wp.Forenames = @PlayerForenames))
                      OR (@PlayerColour = 'Black' AND bp.Surname = @PlayerSurname AND (@PlayerForenames IS NULL OR bp.Forenames = @PlayerForenames))
                  )
            ),
            PlayerMoves AS
            (
                SELECT fg.GameId,
                       SUM(CASE WHEN m.CapturedPiece IS NOT NULL THEN 1 ELSE 0 END) AS CaptureCount,
                       COUNT(*) AS MoveCount
                FROM FilteredGames fg
                INNER JOIN dbo.GameMove m ON m.GameId = fg.GameId
                WHERE fg.PlayerSide IS NOT NULL
                  AND m.MovingSide = fg.PlayerSide
                  AND (@MinPlyIndex IS NULL OR m.PlyIndex >= @MinPlyIndex)
                  AND (@MaxPlyIndex IS NULL OR m.PlyIndex <= @MaxPlyIndex)
                GROUP BY fg.GameId
                HAVING COUNT(*) > 0
            ),
            PerGame AS
            (
                SELECT GameId,
                       CAST(CaptureCount AS FLOAT) / MoveCount AS CaptureRate
                FROM PlayerMoves
            )
            SELECT @PlayerSurname AS PlayerSurname,
                   @PlayerForenames AS PlayerForenames,
                   COUNT(*) AS GameCount,
                   AVG(CaptureRate) AS AverageCaptureRate
            FROM PerGame;
            """;

        /// <summary>
        /// Proportion of games where queens are no longer both on the board on or before a ply threshold.
        /// </summary>
        public static string GetQueenTradeRate =>
            """
            WITH FilteredGames AS
            (
                SELECT g.Id AS GameId,
                       CASE
                           WHEN wp.Surname = @PlayerSurname AND (@PlayerForenames IS NULL OR wp.Forenames = @PlayerForenames) THEN CAST('W' AS CHAR(1))
                           WHEN bp.Surname = @PlayerSurname AND (@PlayerForenames IS NULL OR bp.Forenames = @PlayerForenames) THEN CAST('B' AS CHAR(1))
                           ELSE NULL
                       END AS PlayerSide
                FROM dbo.Game g
                INNER JOIN dbo.Player wp ON wp.Id = g.WhitePlayerId
                INNER JOIN dbo.Player bp ON bp.Id = g.BlackPlayerId
                WHERE (@MinGameYear IS NULL OR (g.GameYear IS NOT NULL AND g.GameYear >= @MinGameYear))
                  AND (@MaxGameYear IS NULL OR (g.GameYear IS NOT NULL AND g.GameYear <= @MaxGameYear))
                  AND (@Eco IS NULL OR g.Eco = @Eco)
                  AND (
                      (@PlayerColour = 'Any' AND (
                          (wp.Surname = @PlayerSurname AND (@PlayerForenames IS NULL OR wp.Forenames = @PlayerForenames))
                          OR (bp.Surname = @PlayerSurname AND (@PlayerForenames IS NULL OR bp.Forenames = @PlayerForenames))
                      ))
                      OR (@PlayerColour = 'White' AND wp.Surname = @PlayerSurname AND (@PlayerForenames IS NULL OR wp.Forenames = @PlayerForenames))
                      OR (@PlayerColour = 'Black' AND bp.Surname = @PlayerSurname AND (@PlayerForenames IS NULL OR bp.Forenames = @PlayerForenames))
                  )
            ),
            FirstQueenTrade AS
            (
                SELECT fg.GameId, MIN(s.PlyIndex) AS QueenTradePly
                FROM FilteredGames fg
                INNER JOIN dbo.GamePositionSummary s ON s.GameId = fg.GameId
                WHERE fg.PlayerSide IS NOT NULL
                  AND (s.WhiteQueenCount <> 1 OR s.BlackQueenCount <> 1)
                GROUP BY fg.GameId
            ),
            PerGame AS
            (
                SELECT fg.GameId,
                       CASE
                           WHEN ft.QueenTradePly IS NOT NULL AND ft.QueenTradePly <= @QueenTradeMaxPly THEN 1.0
                           ELSE 0.0
                       END AS EarlyQueenTrade
                FROM FilteredGames fg
                LEFT JOIN FirstQueenTrade ft ON ft.GameId = fg.GameId
                WHERE fg.PlayerSide IS NOT NULL
            )
            SELECT @PlayerSurname AS PlayerSurname,
                   @PlayerForenames AS PlayerForenames,
                   COUNT(*) AS GameCount,
                   AVG(EarlyQueenTrade) AS QueenTradeRate,
                   @QueenTradeMaxPly AS QueenTradeMaxPly
            FROM PerGame;
            """;

        /// <summary>
        /// Games that have at least one board snapshot but no derived move rows yet (PLAN §5.3.5).
        /// </summary>
        public static string GetGameIdsNeedingAnalyticsBackfill =>
            """
            SELECT g.Id
            FROM dbo.Game g
            WHERE EXISTS (SELECT 1 FROM dbo.BoardPosition bp WHERE bp.GameId = g.Id)
              AND NOT EXISTS (SELECT 1 FROM dbo.GameMove gm WHERE gm.GameId = g.Id)
            ORDER BY g.Id;
            """;

        public static string GetBoardPositionsForGameOrdered =>
            """
            SELECT PlyIndex, WP, WN, WB, WR, WQ, WK, BP, BN, BB, BR, BQ, BK, EnPassantTargetFile
            FROM dbo.BoardPosition
            WHERE GameId = @GameId
            ORDER BY PlyIndex;
            """;
    }
}

