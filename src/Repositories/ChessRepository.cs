using Dapper;
using Interfaces.DTO;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Reflection.PortableExecutable;

namespace Repositories
{
    public interface IChessRepository
    {
        IConfiguration Configuration { get; }

        Task<List<Game>> GetGames();

        Task<List<string>> GetProcessedGameIds();

        Task<int> InsertGame(Game game);
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
        /// <returns></returns>
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
        /// <returns></returns>
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
    }
}
