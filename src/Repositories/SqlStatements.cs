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
    }
}

