namespace Repositories
{
	public class SqlStatements
	{
		public SqlStatements()
		{
		}

        public static string InsertGame =>
        """
        INSERT INTO dbo.Game
        (
            Name,
            GameId
        )
        VALUES
        (@Name, @GameId);
        SELECT CAST(scope_identity() AS int);
        """;
    }
}

