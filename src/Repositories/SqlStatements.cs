namespace Repositories
{
	public class SqlStatements
	{
		public SqlStatements()
		{
		}

        public static string InsertGame =>
        """
        IF (NOT EXISTS (SELECT TOP 1 Id FROM dbo.Game WHERE GameId = @GameId))
        BEGIN
            INSERT INTO dbo.Game (Name, GameId)
            VALUES (@Name, @GameId);
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
    }
}

