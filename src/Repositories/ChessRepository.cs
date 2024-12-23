﻿using System;
using System.Data;
using Dapper;
using Interfaces.DTO;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Repositories
{
    public interface IChessRepository
    {
        IConfiguration Configuration { get; }

        Task<List<Game>> GetGames();

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

        public SqlConnection GetOpenConnection()
        {
            var cs = new SqlConnection(_config.GetConnectionString("ChessConnection"));
            cs.Open();
            return cs;
        }

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
