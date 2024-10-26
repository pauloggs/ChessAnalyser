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
                var queryResult = connection.Query<Game>("SELECT * FROM dbo.[Game]");

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
    }
}
