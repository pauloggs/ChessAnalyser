namespace Repositories
{
	public class SqlStatements
	{
		public SqlStatements()
		{
		}

        public static string GetPlayers =>
            """
            SELECT Id, Surname, Forenames, WasWorldChampion,
                   FideId, Federation, Sex, FideTitle, BirthYear
            FROM dbo.Player;
            """;

        public static string GetPlayerIdBySurnameAndForenames =>
            "SELECT Id FROM dbo.Player WHERE Surname = @Surname AND Forenames = @Forenames;";

        public static string GetPlayersBySurname =>
            """
            SELECT Id, Surname, Forenames, WasWorldChampion,
                   FideId, Federation, Sex, FideTitle, BirthYear
            FROM dbo.Player
            WHERE LOWER(Surname) = LOWER(@Surname);
            """;

        public static string InsertPlayer =>
            """
            INSERT INTO dbo.Player (Surname, Forenames, WasWorldChampion)
            VALUES (@Surname, @Forenames, @WasWorldChampion);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """;

        public static string UpdatePlayerWasWorldChampion =>
            """
            UPDATE dbo.Player
            SET WasWorldChampion = @WasWorldChampion
            WHERE Id = @Id;
            """;

        public static string UpdatePlayerFideMetadata =>
            """
            UPDATE dbo.Player
            SET FideId = @FideId,
                Federation = @Federation,
                Sex = @Sex,
                FideTitle = @FideTitle,
                BirthYear = @BirthYear
            WHERE Id = @Id;
            """;

        public static string GetWorldChampions =>
            """
            SELECT Id, Surname, Forenames, ChampionOrder, ReignStartYear, ReignEndYear
            FROM Ref.WorldChampion
            ORDER BY ChampionOrder, Surname, Forenames;
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
                   CAST(SUM(a.WinCount) AS FLOAT) + (CAST(SUM(a.DrawCount) AS FLOAT) / 2.0) AS Score,
                   CASE
                       WHEN SUM(a.WinCount) + SUM(a.LossCount) + SUM(a.DrawCount) = 0 THEN NULL
                       ELSE (CAST(SUM(a.WinCount) AS FLOAT) + (CAST(SUM(a.DrawCount) AS FLOAT) / 2.0))
                            / CAST(SUM(a.WinCount) + SUM(a.LossCount) + SUM(a.DrawCount) AS FLOAT)
                            * 100.0
                   END AS ScorePercentage
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
        /// Per-player mean first-castling ply for corpus benchmarks (PLAN §12.7).
        /// </summary>
        public static string GetPerPlayerAverageCastlingPly =>
            """
            WITH FilteredGames AS
            (
                SELECT g.Id AS GameId,
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
            FirstCastle AS
            (
                SELECT a.PlayerSurname,
                       a.PlayerForenames,
                       a.GameId,
                       MIN(m.PlyIndex) AS CastlingPly
                FROM Appearances a
                INNER JOIN dbo.GameMove m ON m.GameId = a.GameId
                WHERE (m.IsCastlingKingside = 1 OR m.IsCastlingQueenside = 1)
                  AND m.MovingSide = a.PlayerSide
                GROUP BY a.PlayerSurname, a.PlayerForenames, a.GameId
            )
            SELECT PlayerSurname,
                   PlayerForenames,
                   COUNT(*) AS GameCount,
                   AVG(CAST(CastlingPly AS FLOAT)) AS MetricValue
            FROM FirstCastle
            GROUP BY PlayerSurname, PlayerForenames
            ORDER BY MetricValue DESC, PlayerSurname, PlayerForenames;
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
        /// Per-player mean minor-piece composition for corpus benchmarks (PLAN §12.7).
        /// </summary>
        public static string GetPerPlayerMinorPieceComposition =>
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
                       CASE a.PlayerSide
                           WHEN 'W' THEN CAST(s.WhiteBishopCount - s.WhiteKnightCount AS FLOAT)
                           WHEN 'B' THEN CAST(s.BlackBishopCount - s.BlackKnightCount AS FLOAT)
                       END AS MinorPieceDelta
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
                       AVG(MinorPieceDelta) AS AvgMinorPieceDelta
                FROM PerPly
                GROUP BY PlayerSurname, PlayerForenames, GameId
                HAVING COUNT(*) > 0
            )
            SELECT PlayerSurname,
                   PlayerForenames,
                   COUNT(*) AS GameCount,
                   AVG(AvgMinorPieceDelta) AS MetricValue
            FROM PerGame
            GROUP BY PlayerSurname, PlayerForenames
            ORDER BY MetricValue DESC, PlayerSurname, PlayerForenames;
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
        /// Per-player mean capture rate for corpus benchmarks (PLAN §12.7).
        /// </summary>
        public static string GetPerPlayerCaptureRate =>
            """
            WITH FilteredGames AS
            (
                SELECT g.Id AS GameId,
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
            PlayerMoves AS
            (
                SELECT a.PlayerSurname,
                       a.PlayerForenames,
                       a.GameId,
                       SUM(CASE WHEN m.CapturedPiece IS NOT NULL THEN 1 ELSE 0 END) AS CaptureCount,
                       COUNT(*) AS MoveCount
                FROM Appearances a
                INNER JOIN dbo.GameMove m ON m.GameId = a.GameId
                WHERE m.MovingSide = a.PlayerSide
                  AND (@MinPlyIndex IS NULL OR m.PlyIndex >= @MinPlyIndex)
                  AND (@MaxPlyIndex IS NULL OR m.PlyIndex <= @MaxPlyIndex)
                GROUP BY a.PlayerSurname, a.PlayerForenames, a.GameId
                HAVING COUNT(*) > 0
            ),
            PerGame AS
            (
                SELECT PlayerSurname,
                       PlayerForenames,
                       GameId,
                       CAST(CaptureCount AS FLOAT) / MoveCount AS CaptureRate
                FROM PlayerMoves
            )
            SELECT PlayerSurname,
                   PlayerForenames,
                   COUNT(*) AS GameCount,
                   AVG(CaptureRate) AS MetricValue
            FROM PerGame
            GROUP BY PlayerSurname, PlayerForenames
            ORDER BY MetricValue DESC, PlayerSurname, PlayerForenames;
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
        /// Per-player early queen trade rate for corpus benchmarks (PLAN §12.7).
        /// </summary>
        public static string GetPerPlayerQueenTradeRate =>
            """
            WITH FilteredGames AS
            (
                SELECT g.Id AS GameId,
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
            FirstQueenTrade AS
            (
                SELECT fg.GameId,
                       MIN(s.PlyIndex) AS QueenTradePly
                FROM FilteredGames fg
                INNER JOIN dbo.GamePositionSummary s ON s.GameId = fg.GameId
                WHERE s.WhiteQueenCount <> 1 OR s.BlackQueenCount <> 1
                GROUP BY fg.GameId
            ),
            PerGame AS
            (
                SELECT a.PlayerSurname,
                       a.PlayerForenames,
                       a.GameId,
                       CASE
                           WHEN ft.QueenTradePly IS NOT NULL AND ft.QueenTradePly <= @QueenTradeMaxPly THEN 1.0
                           ELSE 0.0
                       END AS EarlyQueenTrade
                FROM Appearances a
                LEFT JOIN FirstQueenTrade ft ON ft.GameId = a.GameId
            )
            SELECT PlayerSurname,
                   PlayerForenames,
                   COUNT(*) AS GameCount,
                   AVG(EarlyQueenTrade) AS MetricValue
            FROM PerGame
            GROUP BY PlayerSurname, PlayerForenames
            ORDER BY MetricValue DESC, PlayerSurname, PlayerForenames;
            """;

        /// <summary>
        /// Mean per-game share of moves to central squares d4, d5, e4, e5 (ToSquare 27, 28, 35, 36).
        /// </summary>
        public static string GetCentreMoveRate =>
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
                       SUM(CASE WHEN m.ToSquare IN (27, 28, 35, 36) THEN 1 ELSE 0 END) AS CentreCount,
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
                       CAST(CentreCount AS FLOAT) / MoveCount AS CentreMoveRate
                FROM PlayerMoves
            )
            SELECT @PlayerSurname AS PlayerSurname,
                   @PlayerForenames AS PlayerForenames,
                   COUNT(*) AS GameCount,
                   AVG(CentreMoveRate) AS AverageCentreMoveRate
            FROM PerGame;
            """;

        /// <summary>
        /// Mean per-game share of moves into the opponent's half (White: rank index ≥ 4; Black: rank index ≤ 3).
        /// </summary>
        public static string GetForwardMoveRate =>
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
                       SUM(CASE
                               WHEN m.MovingSide = 'W' AND (m.ToSquare / 8) >= 4 THEN 1
                               WHEN m.MovingSide = 'B' AND (m.ToSquare / 8) <= 3 THEN 1
                               ELSE 0
                           END) AS ForwardCount,
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
                       CAST(ForwardCount AS FLOAT) / MoveCount AS ForwardMoveRate
                FROM PlayerMoves
            )
            SELECT @PlayerSurname AS PlayerSurname,
                   @PlayerForenames AS PlayerForenames,
                   COUNT(*) AS GameCount,
                   AVG(ForwardMoveRate) AS AverageForwardMoveRate
            FROM PerGame;
            """;

        /// <summary>
        /// Kingside vs queenside rates on the filtered player's first castle per game.
        /// </summary>
        public static string GetCastlingSidePreference =>
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
            FirstCastlePly AS
            (
                SELECT fg.GameId,
                       MIN(m.PlyIndex) AS CastlingPly
                FROM FilteredGames fg
                INNER JOIN dbo.GameMove m ON m.GameId = fg.GameId
                WHERE fg.PlayerSide IS NOT NULL
                  AND m.MovingSide = fg.PlayerSide
                  AND (m.IsCastlingKingside = 1 OR m.IsCastlingQueenside = 1)
                GROUP BY fg.GameId
            ),
            PerGame AS
            (
                SELECT fcp.GameId,
                       CASE WHEN m.IsCastlingKingside = 1 THEN 1.0 ELSE 0.0 END AS IsKingside,
                       CASE WHEN m.IsCastlingQueenside = 1 THEN 1.0 ELSE 0.0 END AS IsQueenside
                FROM FirstCastlePly fcp
                INNER JOIN FilteredGames fg ON fg.GameId = fcp.GameId
                INNER JOIN dbo.GameMove m ON m.GameId = fcp.GameId AND m.PlyIndex = fcp.CastlingPly
                WHERE m.MovingSide = fg.PlayerSide
                  AND (m.IsCastlingKingside = 1 OR m.IsCastlingQueenside = 1)
            )
            SELECT @PlayerSurname AS PlayerSurname,
                   @PlayerForenames AS PlayerForenames,
                   COUNT(*) AS GamesWithCastling,
                   AVG(IsKingside) AS KingsideRate,
                   AVG(IsQueenside) AS QueensideRate
            FROM PerGame;
            """;

        /// <summary>
        /// Proportion of filtered games where White and Black each castled to opposite wings.
        /// </summary>
        public static string GetOppositeSideCastlingRate =>
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
            WhiteFirstCastlePly AS
            (
                SELECT fg.GameId,
                       MIN(m.PlyIndex) AS CastlingPly
                FROM FilteredGames fg
                INNER JOIN dbo.GameMove m ON m.GameId = fg.GameId
                WHERE m.MovingSide = 'W'
                  AND (m.IsCastlingKingside = 1 OR m.IsCastlingQueenside = 1)
                GROUP BY fg.GameId
            ),
            WhiteCastle AS
            (
                SELECT wcp.GameId,
                       m.IsCastlingKingside
                FROM WhiteFirstCastlePly wcp
                INNER JOIN dbo.GameMove m ON m.GameId = wcp.GameId AND m.PlyIndex = wcp.CastlingPly
                WHERE m.MovingSide = 'W'
                  AND (m.IsCastlingKingside = 1 OR m.IsCastlingQueenside = 1)
            ),
            BlackFirstCastlePly AS
            (
                SELECT fg.GameId,
                       MIN(m.PlyIndex) AS CastlingPly
                FROM FilteredGames fg
                INNER JOIN dbo.GameMove m ON m.GameId = fg.GameId
                WHERE m.MovingSide = 'B'
                  AND (m.IsCastlingKingside = 1 OR m.IsCastlingQueenside = 1)
                GROUP BY fg.GameId
            ),
            BlackCastle AS
            (
                SELECT bcp.GameId,
                       m.IsCastlingKingside
                FROM BlackFirstCastlePly bcp
                INNER JOIN dbo.GameMove m ON m.GameId = bcp.GameId AND m.PlyIndex = bcp.CastlingPly
                WHERE m.MovingSide = 'B'
                  AND (m.IsCastlingKingside = 1 OR m.IsCastlingQueenside = 1)
            ),
            EligibleGames AS
            (
                SELECT fg.GameId,
                       CASE
                           WHEN wc.IsCastlingKingside <> bc.IsCastlingKingside THEN 1.0
                           ELSE 0.0
                       END AS IsOppositeSide
                FROM FilteredGames fg
                INNER JOIN WhiteCastle wc ON wc.GameId = fg.GameId
                INNER JOIN BlackCastle bc ON bc.GameId = fg.GameId
            )
            SELECT @PlayerSurname AS PlayerSurname,
                   @PlayerForenames AS PlayerForenames,
                   COUNT(*) AS EligibleGameCount,
                   AVG(IsOppositeSide) AS OppositeSideCastlingRate
            FROM EligibleGames;
            """;

        /// <summary>
        /// Proportion of filtered games where the player never castled.
        /// </summary>
        public static string GetUncastledKingRate =>
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
            PerGame AS
            (
                SELECT fg.GameId,
                       CASE
                           WHEN EXISTS (
                               SELECT 1
                               FROM dbo.GameMove m
                               WHERE m.GameId = fg.GameId
                                 AND m.MovingSide = fg.PlayerSide
                                 AND (m.IsCastlingKingside = 1 OR m.IsCastlingQueenside = 1)
                           ) THEN 0.0
                           ELSE 1.0
                       END AS IsUncastled
                FROM FilteredGames fg
                WHERE fg.PlayerSide IS NOT NULL
            )
            SELECT @PlayerSurname AS PlayerSurname,
                   @PlayerForenames AS PlayerForenames,
                   COUNT(*) AS GameCount,
                   AVG(IsUncastled) AS UncastledKingRate
            FROM PerGame;
            """;

        /// <summary>
        /// Mean ply of the filtered player's first queen move; games without a queen move are excluded.
        /// </summary>
        public static string GetFirstQueenMovePly =>
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
            FirstQueenMove AS
            (
                SELECT fg.GameId,
                       MIN(m.PlyIndex) AS FirstQueenPly
                FROM FilteredGames fg
                INNER JOIN dbo.GameMove m ON m.GameId = fg.GameId
                WHERE fg.PlayerSide IS NOT NULL
                  AND m.MovingSide = fg.PlayerSide
                  AND m.MovedPiece = 'Q'
                GROUP BY fg.GameId
            )
            SELECT @PlayerSurname AS PlayerSurname,
                   @PlayerForenames AS PlayerForenames,
                   COUNT(*) AS GamesWithQueenMove,
                   AVG(CAST(FirstQueenPly AS FLOAT)) AS AverageFirstQueenMovePly
            FROM FirstQueenMove;
            """;

        /// <summary>
        /// Per-player mean centre-move rate for corpus benchmarks (PLAN §12.7).
        /// </summary>
        public static string GetPerPlayerCentreMoveRate =>
            """
            WITH FilteredGames AS
            (
                SELECT g.Id AS GameId,
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
            PlayerMoves AS
            (
                SELECT a.PlayerSurname,
                       a.PlayerForenames,
                       a.GameId,
                       SUM(CASE WHEN m.ToSquare IN (27, 28, 35, 36) THEN 1 ELSE 0 END) AS CentreCount,
                       COUNT(*) AS MoveCount
                FROM Appearances a
                INNER JOIN dbo.GameMove m ON m.GameId = a.GameId
                WHERE m.MovingSide = a.PlayerSide
                  AND (@MinPlyIndex IS NULL OR m.PlyIndex >= @MinPlyIndex)
                  AND (@MaxPlyIndex IS NULL OR m.PlyIndex <= @MaxPlyIndex)
                GROUP BY a.PlayerSurname, a.PlayerForenames, a.GameId
                HAVING COUNT(*) > 0
            ),
            PerGame AS
            (
                SELECT PlayerSurname,
                       PlayerForenames,
                       GameId,
                       CAST(CentreCount AS FLOAT) / MoveCount AS CentreMoveRate
                FROM PlayerMoves
            )
            SELECT PlayerSurname,
                   PlayerForenames,
                   COUNT(*) AS GameCount,
                   AVG(CentreMoveRate) AS MetricValue
            FROM PerGame
            GROUP BY PlayerSurname, PlayerForenames
            ORDER BY MetricValue DESC, PlayerSurname, PlayerForenames;
            """;

        /// <summary>
        /// Per-player mean forward-move rate for corpus benchmarks (PLAN §12.7).
        /// </summary>
        public static string GetPerPlayerForwardMoveRate =>
            """
            WITH FilteredGames AS
            (
                SELECT g.Id AS GameId,
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
            PlayerMoves AS
            (
                SELECT a.PlayerSurname,
                       a.PlayerForenames,
                       a.GameId,
                       SUM(CASE
                               WHEN m.MovingSide = 'W' AND (m.ToSquare / 8) >= 4 THEN 1
                               WHEN m.MovingSide = 'B' AND (m.ToSquare / 8) <= 3 THEN 1
                               ELSE 0
                           END) AS ForwardCount,
                       COUNT(*) AS MoveCount
                FROM Appearances a
                INNER JOIN dbo.GameMove m ON m.GameId = a.GameId
                WHERE m.MovingSide = a.PlayerSide
                  AND (@MinPlyIndex IS NULL OR m.PlyIndex >= @MinPlyIndex)
                  AND (@MaxPlyIndex IS NULL OR m.PlyIndex <= @MaxPlyIndex)
                GROUP BY a.PlayerSurname, a.PlayerForenames, a.GameId
                HAVING COUNT(*) > 0
            ),
            PerGame AS
            (
                SELECT PlayerSurname,
                       PlayerForenames,
                       GameId,
                       CAST(ForwardCount AS FLOAT) / MoveCount AS ForwardMoveRate
                FROM PlayerMoves
            )
            SELECT PlayerSurname,
                   PlayerForenames,
                   COUNT(*) AS GameCount,
                   AVG(ForwardMoveRate) AS MetricValue
            FROM PerGame
            GROUP BY PlayerSurname, PlayerForenames
            ORDER BY MetricValue DESC, PlayerSurname, PlayerForenames;
            """;

        /// <summary>
        /// Per-player kingside castling preference for corpus benchmarks (PLAN §12.7).
        /// </summary>
        public static string GetPerPlayerCastlingSidePreference =>
            """
            WITH FilteredGames AS
            (
                SELECT g.Id AS GameId,
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
            FirstCastlePly AS
            (
                SELECT a.PlayerSurname,
                       a.PlayerForenames,
                       a.GameId,
                       MIN(m.PlyIndex) AS CastlingPly
                FROM Appearances a
                INNER JOIN dbo.GameMove m ON m.GameId = a.GameId
                WHERE m.MovingSide = a.PlayerSide
                  AND (m.IsCastlingKingside = 1 OR m.IsCastlingQueenside = 1)
                GROUP BY a.PlayerSurname, a.PlayerForenames, a.GameId
            ),
            PerGame AS
            (
                SELECT fcp.PlayerSurname,
                       fcp.PlayerForenames,
                       fcp.GameId,
                       CASE WHEN m.IsCastlingKingside = 1 THEN 1.0 ELSE 0.0 END AS IsKingside
                FROM FirstCastlePly fcp
                INNER JOIN Appearances a ON a.GameId = fcp.GameId
                    AND a.PlayerSurname = fcp.PlayerSurname
                    AND ((a.PlayerForenames IS NULL AND fcp.PlayerForenames IS NULL) OR a.PlayerForenames = fcp.PlayerForenames)
                INNER JOIN dbo.GameMove m ON m.GameId = fcp.GameId AND m.PlyIndex = fcp.CastlingPly
                WHERE m.MovingSide = a.PlayerSide
                  AND (m.IsCastlingKingside = 1 OR m.IsCastlingQueenside = 1)
            )
            SELECT PlayerSurname,
                   PlayerForenames,
                   COUNT(*) AS GameCount,
                   AVG(IsKingside) AS MetricValue
            FROM PerGame
            GROUP BY PlayerSurname, PlayerForenames
            ORDER BY MetricValue DESC, PlayerSurname, PlayerForenames;
            """;

        /// <summary>
        /// Per-player opposite-side castling rate for corpus benchmarks (PLAN §12.7).
        /// </summary>
        public static string GetPerPlayerOppositeSideCastlingRate =>
            """
            WITH FilteredGames AS
            (
                SELECT g.Id AS GameId,
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
            WhiteFirstCastlePly AS
            (
                SELECT fg.GameId,
                       MIN(m.PlyIndex) AS CastlingPly
                FROM FilteredGames fg
                INNER JOIN dbo.GameMove m ON m.GameId = fg.GameId
                WHERE m.MovingSide = 'W'
                  AND (m.IsCastlingKingside = 1 OR m.IsCastlingQueenside = 1)
                GROUP BY fg.GameId
            ),
            WhiteCastle AS
            (
                SELECT wcp.GameId,
                       m.IsCastlingKingside
                FROM WhiteFirstCastlePly wcp
                INNER JOIN dbo.GameMove m ON m.GameId = wcp.GameId AND m.PlyIndex = wcp.CastlingPly
                WHERE m.MovingSide = 'W'
                  AND (m.IsCastlingKingside = 1 OR m.IsCastlingQueenside = 1)
            ),
            BlackFirstCastlePly AS
            (
                SELECT fg.GameId,
                       MIN(m.PlyIndex) AS CastlingPly
                FROM FilteredGames fg
                INNER JOIN dbo.GameMove m ON m.GameId = fg.GameId
                WHERE m.MovingSide = 'B'
                  AND (m.IsCastlingKingside = 1 OR m.IsCastlingQueenside = 1)
                GROUP BY fg.GameId
            ),
            BlackCastle AS
            (
                SELECT bcp.GameId,
                       m.IsCastlingKingside
                FROM BlackFirstCastlePly bcp
                INNER JOIN dbo.GameMove m ON m.GameId = bcp.GameId AND m.PlyIndex = bcp.CastlingPly
                WHERE m.MovingSide = 'B'
                  AND (m.IsCastlingKingside = 1 OR m.IsCastlingQueenside = 1)
            ),
            EligibleGames AS
            (
                SELECT fg.GameId,
                       CASE
                           WHEN wc.IsCastlingKingside <> bc.IsCastlingKingside THEN 1.0
                           ELSE 0.0
                       END AS IsOppositeSide
                FROM FilteredGames fg
                INNER JOIN WhiteCastle wc ON wc.GameId = fg.GameId
                INNER JOIN BlackCastle bc ON bc.GameId = fg.GameId
            ),
            PerGame AS
            (
                SELECT a.PlayerSurname,
                       a.PlayerForenames,
                       a.GameId,
                       eg.IsOppositeSide
                FROM Appearances a
                INNER JOIN EligibleGames eg ON eg.GameId = a.GameId
            )
            SELECT PlayerSurname,
                   PlayerForenames,
                   COUNT(*) AS GameCount,
                   AVG(IsOppositeSide) AS MetricValue
            FROM PerGame
            GROUP BY PlayerSurname, PlayerForenames
            ORDER BY MetricValue DESC, PlayerSurname, PlayerForenames;
            """;

        /// <summary>
        /// Per-player uncastled-king rate for corpus benchmarks (PLAN §12.7).
        /// </summary>
        public static string GetPerPlayerUncastledKingRate =>
            """
            WITH FilteredGames AS
            (
                SELECT g.Id AS GameId,
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
            PerGame AS
            (
                SELECT a.PlayerSurname,
                       a.PlayerForenames,
                       a.GameId,
                       CASE
                           WHEN EXISTS (
                               SELECT 1
                               FROM dbo.GameMove m
                               WHERE m.GameId = a.GameId
                                 AND m.MovingSide = a.PlayerSide
                                 AND (m.IsCastlingKingside = 1 OR m.IsCastlingQueenside = 1)
                           ) THEN 0.0
                           ELSE 1.0
                       END AS IsUncastled
                FROM Appearances a
            )
            SELECT PlayerSurname,
                   PlayerForenames,
                   COUNT(*) AS GameCount,
                   AVG(IsUncastled) AS MetricValue
            FROM PerGame
            GROUP BY PlayerSurname, PlayerForenames
            ORDER BY MetricValue DESC, PlayerSurname, PlayerForenames;
            """;

        /// <summary>
        /// Per-player mean first queen move ply for corpus benchmarks (PLAN §12.7).
        /// </summary>
        public static string GetPerPlayerFirstQueenMovePly =>
            """
            WITH FilteredGames AS
            (
                SELECT g.Id AS GameId,
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
            FirstQueenMove AS
            (
                SELECT a.PlayerSurname,
                       a.PlayerForenames,
                       a.GameId,
                       MIN(m.PlyIndex) AS FirstQueenPly
                FROM Appearances a
                INNER JOIN dbo.GameMove m ON m.GameId = a.GameId
                WHERE m.MovingSide = a.PlayerSide
                  AND m.MovedPiece = 'Q'
                GROUP BY a.PlayerSurname, a.PlayerForenames, a.GameId
            )
            SELECT PlayerSurname,
                   PlayerForenames,
                   COUNT(*) AS GameCount,
                   AVG(CAST(FirstQueenPly AS FLOAT)) AS MetricValue
            FROM FirstQueenMove
            GROUP BY PlayerSurname, PlayerForenames
            ORDER BY MetricValue DESC, PlayerSurname, PlayerForenames;
            """;

        /// <summary>
        /// Mean game length in plies for games involving the filtered player.
        /// </summary>
        public static string GetAverageGameLength =>
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
            GameLength AS
            (
                SELECT fg.GameId,
                       MAX(s.PlyIndex) AS MaxPly
                FROM FilteredGames fg
                INNER JOIN dbo.GamePositionSummary s ON s.GameId = fg.GameId
                WHERE fg.PlayerSide IS NOT NULL
                GROUP BY fg.GameId
            )
            SELECT @PlayerSurname AS PlayerSurname,
                   @PlayerForenames AS PlayerForenames,
                   COUNT(*) AS GameCount,
                   AVG(CAST(MaxPly AS FLOAT)) AS AverageGameLengthPly
            FROM GameLength;
            """;

        /// <summary>
        /// Per-player mean game length for corpus benchmarks (PLAN §12.7).
        /// </summary>
        public static string GetPerPlayerAverageGameLength =>
            """
            WITH FilteredGames AS
            (
                SELECT g.Id AS GameId,
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
                       fg.WhiteForenames AS PlayerForenames
                FROM FilteredGames fg
                WHERE @PlayerColour = 'Any' OR @PlayerColour = 'White'

                UNION ALL

                SELECT fg.GameId,
                       fg.BlackSurname,
                       fg.BlackForenames
                FROM FilteredGames fg
                WHERE @PlayerColour = 'Any' OR @PlayerColour = 'Black'
            ),
            GameLength AS
            (
                SELECT a.PlayerSurname,
                       a.PlayerForenames,
                       a.GameId,
                       MAX(s.PlyIndex) AS MaxPly
                FROM Appearances a
                INNER JOIN dbo.GamePositionSummary s ON s.GameId = a.GameId
                GROUP BY a.PlayerSurname, a.PlayerForenames, a.GameId
            )
            SELECT PlayerSurname,
                   PlayerForenames,
                   COUNT(*) AS GameCount,
                   AVG(CAST(MaxPly AS FLOAT)) AS MetricValue
            FROM GameLength
            GROUP BY PlayerSurname, PlayerForenames
            ORDER BY MetricValue DESC, PlayerSurname, PlayerForenames;
            """;

        /// <summary>
        /// Share of the filtered player's draws that end at or below a ply threshold.
        /// </summary>
        public static string GetShortDrawRate =>
            """
            WITH FilteredGames AS
            (
                SELECT g.Id AS GameId,
                       g.Winner,
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
            DrawGames AS
            (
                SELECT fg.GameId,
                       MAX(s.PlyIndex) AS MaxPly
                FROM FilteredGames fg
                INNER JOIN dbo.GamePositionSummary s ON s.GameId = fg.GameId
                WHERE fg.PlayerSide IS NOT NULL
                  AND fg.Winner = 'D'
                GROUP BY fg.GameId
            ),
            PerDraw AS
            (
                SELECT CASE
                           WHEN MaxPly <= @ShortDrawMaxPly THEN 1.0
                           ELSE 0.0
                       END AS IsShortDraw
                FROM DrawGames
            )
            SELECT @PlayerSurname AS PlayerSurname,
                   @PlayerForenames AS PlayerForenames,
                   COUNT(*) AS DrawCount,
                   AVG(IsShortDraw) AS ShortDrawRate,
                   @ShortDrawMaxPly AS ShortDrawMaxPly
            FROM PerDraw;
            """;

        /// <summary>
        /// Per-player short-draw rate for corpus benchmarks (PLAN §12.7).
        /// </summary>
        public static string GetPerPlayerShortDrawRate =>
            """
            WITH FilteredGames AS
            (
                SELECT g.Id AS GameId,
                       g.Winner,
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
                       fg.WhiteForenames AS PlayerForenames
                FROM FilteredGames fg
                WHERE @PlayerColour = 'Any' OR @PlayerColour = 'White'

                UNION ALL

                SELECT fg.GameId,
                       fg.BlackSurname,
                       fg.BlackForenames
                FROM FilteredGames fg
                WHERE @PlayerColour = 'Any' OR @PlayerColour = 'Black'
            ),
            DrawGames AS
            (
                SELECT a.PlayerSurname,
                       a.PlayerForenames,
                       a.GameId,
                       MAX(s.PlyIndex) AS MaxPly
                FROM Appearances a
                INNER JOIN dbo.Game g ON g.Id = a.GameId
                INNER JOIN dbo.GamePositionSummary s ON s.GameId = a.GameId
                WHERE g.Winner = 'D'
                GROUP BY a.PlayerSurname, a.PlayerForenames, a.GameId
            )
            SELECT PlayerSurname,
                   PlayerForenames,
                   COUNT(*) AS GameCount,
                   AVG(CASE
                           WHEN MaxPly <= @ShortDrawMaxPly THEN 1.0
                           ELSE 0.0
                       END) AS MetricValue
            FROM DrawGames
            GROUP BY PlayerSurname, PlayerForenames
            ORDER BY MetricValue DESC, PlayerSurname, PlayerForenames;
            """;

        /// <summary>
        /// Count of distinct ECO codes in games involving the filtered player.
        /// </summary>
        public static string GetEcoDiversity =>
            """
            WITH FilteredGames AS
            (
                SELECT g.Id AS GameId,
                       g.Eco,
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
            PlayerGames AS
            (
                SELECT fg.Eco
                FROM FilteredGames fg
                WHERE fg.PlayerSide IS NOT NULL
            )
            SELECT @PlayerSurname AS PlayerSurname,
                   @PlayerForenames AS PlayerForenames,
                   COUNT(*) AS GameCount,
                   COUNT(DISTINCT CASE
                       WHEN Eco IS NOT NULL AND LTRIM(RTRIM(Eco)) <> '' THEN Eco
                   END) AS EcoDiversity
            FROM PlayerGames;
            """;

        /// <summary>
        /// Per-player distinct ECO count for corpus benchmarks (PLAN §12.7).
        /// </summary>
        public static string GetPerPlayerEcoDiversity =>
            """
            WITH FilteredGames AS
            (
                SELECT g.Id AS GameId,
                       g.Eco,
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
                       fg.WhiteForenames AS PlayerForenames
                FROM FilteredGames fg
                WHERE @PlayerColour = 'Any' OR @PlayerColour = 'White'

                UNION ALL

                SELECT fg.GameId,
                       fg.BlackSurname,
                       fg.BlackForenames
                FROM FilteredGames fg
                WHERE @PlayerColour = 'Any' OR @PlayerColour = 'Black'
            )
            SELECT a.PlayerSurname,
                   a.PlayerForenames,
                   COUNT(*) AS GameCount,
                   COUNT(DISTINCT CASE
                       WHEN fg.Eco IS NOT NULL AND LTRIM(RTRIM(fg.Eco)) <> '' THEN fg.Eco
                   END) AS MetricValue
            FROM Appearances a
            INNER JOIN FilteredGames fg ON fg.GameId = a.GameId
            GROUP BY a.PlayerSurname, a.PlayerForenames
            ORDER BY MetricValue DESC, a.PlayerSurname, a.PlayerForenames;
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

