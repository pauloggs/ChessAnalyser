using Dapper;
using Interfaces.DTO;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Linq;

namespace Repositories
{
    public interface IChessRepository
    {
        IConfiguration Configuration { get; }

        Task<List<Game>> GetGames();

        Task<List<string>> GetProcessedGameIds();

        Task<int> InsertGame(Game game);

        /// <summary>
        /// Persists all board positions for a game (initial position and per-ply positions) to dbo.BoardPosition.
        /// Replaces any existing rows for this game.
        /// </summary>
        Task InsertBoardPositions(Game game, int gameId);
    }

    public class ChessRepository : IChessRepository
    {
        private readonly IConfiguration _config;

        public ChessRepository(IConfiguration config)
        {
            _config = config;
        }

        public IConfiguration Configuration => throw new NotImplementedException();

        /// <summary>
        /// Retrieves all games from the database.
        /// </summary>
        public async Task<List<Game>> GetGames()
        {
            var returnValue = new List<Game>();

            var connection = GetOpenConnection();

            using(connection)
            {
                var queryResult = await connection.QueryAsync<Game>("SELECT * FROM dbo.[Game]");

                returnValue = queryResult.ToList();
            }
            
            return returnValue;
        }

        /// <summary>
        /// Gets an open SQL connection.
        /// </summary>
        public SqlConnection GetOpenConnection()
        {
            var cs = new SqlConnection(_config.GetConnectionString("ChessConnection"));
            cs.Open();
            return cs;
        }

        public async Task<List<string>> GetProcessedGameIds()
        {
            try
            {
                List<string> gameIds = [];

                using var connection = GetOpenConnection();

                using (connection)
                {
                    var sql = SqlStatements.GetGameIds;

                    SqlCommand command = new SqlCommand(sql, connection);

                    using (SqlDataReader reader = command.ExecuteReader())
            {
                        while (reader.Read())
                        {
                            gameIds.Add(reader.GetString(0)); // Assuming the column is of type string
                        }
                    }

                    return gameIds ?? [];
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Inserts a game into the database and returns the newly created Game Id.
        /// </summary>
        /// <param name="game"></param>
        /// <exception cref="Exception"></exception>
        public async Task<int> InsertGame(Game game)
        {
            try
            {
                using var connection = GetOpenConnection();

                using (connection)
                {
                    var sql = SqlStatements.InsertGame;

                    var parameters = new { game.Name, game.GameId };

                    var gameId = await connection.ExecuteScalarAsync<int>(sql, parameters);

                    return gameId;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Persists all board positions for a game to dbo.BoardPosition. Replaces any existing rows for this game.
        /// </summary>
        public async Task InsertBoardPositions(Game game, int gameId)
        {
            if (game == null)
                throw new ArgumentNullException(nameof(game));

            using var connection = GetOpenConnection();
            using var transaction = connection.BeginTransaction();

            try
            {
                await connection.ExecuteAsync(
                    SqlStatements.DeleteBoardPositionsForGame,
                    new { GameId = gameId },
                    transaction);

                var positionsToInsert = new List<(int PlyIndex, BoardPosition Position)>();

                if (game.BoardPositions != null)
                {
                    foreach (var kvp in game.BoardPositions.OrderBy(k => k.Key))
                        positionsToInsert.Add((kvp.Key, kvp.Value));
                }
                // Persist initial position if set and not already in BoardPositions (e.g. -1)
                if (game.InitialBoardPosition != null && (game.BoardPositions == null || !game.BoardPositions.ContainsKey(-1)))
                    positionsToInsert.Add((-1, game.InitialBoardPosition));

                foreach (var (plyIndex, position) in positionsToInsert.OrderBy(x => x.PlyIndex))
                {
                    var row = new
                    {
                        GameId = gameId,
                        PlyIndex = plyIndex,
                        WP = (long)position.PiecePositions["WP"],
                        WN = (long)position.PiecePositions["WN"],
                        WB = (long)position.PiecePositions["WB"],
                        WR = (long)position.PiecePositions["WR"],
                        WQ = (long)position.PiecePositions["WQ"],
                        WK = (long)position.PiecePositions["WK"],
                        BP = (long)position.PiecePositions["BP"],
                        BN = (long)position.PiecePositions["BN"],
                        BB = (long)position.PiecePositions["BB"],
                        BR = (long)position.PiecePositions["BR"],
                        BQ = (long)position.PiecePositions["BQ"],
                        BK = (long)position.PiecePositions["BK"],
                        EnPassantTargetFile = position.EnPassantTargetFile.HasValue
                            ? position.EnPassantTargetFile.Value.ToString()
                            : (string?)null
                    };

                    await connection.ExecuteAsync(
                        SqlStatements.InsertBoardPosition,
                        row,
                        transaction);
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
