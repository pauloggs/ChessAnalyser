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
            INSERT INTO dbo.Game
            (
                Name,
                GameId
            )
            VALUES
            (@Name, @GameId);
            SELECT CAST(scope_identity() AS int);
        END;
        """;
    }
}

