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
              AND (@WhitePlayerSurname IS NULL OR (wp.Surname = @WhitePlayerSurname AND (@WhitePlayerForenames IS NULL OR wp.Forenames = @WhitePlayerForenames)))
              AND (@BlackPlayerSurname IS NULL OR (bp.Surname = @BlackPlayerSurname AND (@BlackPlayerForenames IS NULL OR bp.Forenames = @BlackPlayerForenames)))
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
              AND (@WhitePlayerSurname IS NULL OR (wp.Surname = @WhitePlayerSurname AND (@WhitePlayerForenames IS NULL OR wp.Forenames = @WhitePlayerForenames)))
              AND (@BlackPlayerSurname IS NULL OR (bp.Surname = @BlackPlayerSurname AND (@BlackPlayerForenames IS NULL OR bp.Forenames = @BlackPlayerForenames)))
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
              AND (@WhitePlayerSurname IS NULL OR (wp.Surname = @WhitePlayerSurname AND (@WhitePlayerForenames IS NULL OR wp.Forenames = @WhitePlayerForenames)))
              AND (@BlackPlayerSurname IS NULL OR (bp.Surname = @BlackPlayerSurname AND (@BlackPlayerForenames IS NULL OR bp.Forenames = @BlackPlayerForenames)))
              AND (@Eco IS NULL OR g.Eco = @Eco)
            GROUP BY g.Eco
            ORDER BY GameCount DESC, g.Eco;
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

