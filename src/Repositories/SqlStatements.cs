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
            WHERE g.GameYear IS NOT NULL
              AND (@MinGameYear IS NULL OR g.GameYear >= @MinGameYear)
              AND (@MaxGameYear IS NULL OR g.GameYear <= @MaxGameYear)
              AND (@WhitePlayerId IS NULL OR g.WhitePlayerId = @WhitePlayerId)
              AND (@BlackPlayerId IS NULL OR g.BlackPlayerId = @BlackPlayerId)
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
            WHERE m.MovedPiece = 'N'
              AND (@MinGameYear IS NULL OR (g.GameYear IS NOT NULL AND g.GameYear >= @MinGameYear))
              AND (@MaxGameYear IS NULL OR (g.GameYear IS NOT NULL AND g.GameYear <= @MaxGameYear))
              AND (@WhitePlayerId IS NULL OR g.WhitePlayerId = @WhitePlayerId)
              AND (@BlackPlayerId IS NULL OR g.BlackPlayerId = @BlackPlayerId)
              AND (@Eco IS NULL OR g.Eco = @Eco)
            GROUP BY m.ToSquare
            ORDER BY MoveCount DESC, m.ToSquare;
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

