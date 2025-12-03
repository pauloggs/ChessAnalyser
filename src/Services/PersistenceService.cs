using Interfaces.DTO;
using Repositories;
using System.Threading.Tasks;

namespace Services
{
	public interface IPersistenceService
    {
        /// <summary>
        /// For each provided <see cref="Game"/> check if it has already been written
        /// to the database. If it has, remove from the returned list.
        /// </summary>
        /// <param name="games"></param>
        /// <returns></returns>
		Task<List<Game>> GetUnprocessedGames(List<Game> games);

        /// <summary>
        /// Inserts each of the <see cref="Game"/>s into the database.
        /// </summary>
        /// <param name="games"></param>
        /// <returns></returns>
        Task InsertGames(List<Game> games);
	}

	public class PersistenceService(IChessRepository chessRepository) : IPersistenceService
    {
        private readonly IChessRepository chessRepository = chessRepository;

        /// <summary>
        /// Filters out any <see cref="Game"/>s that have already been processed.
        /// </summary>
        /// <param name="games"></param>
        /// <returns></returns>
        public async Task<List<Game>> GetUnprocessedGames(List<Game> games)
        {
            var processedGameIds = await chessRepository.GetProcessedGameIds();

            if (processedGameIds == null || processedGameIds.Count == 0)
            {
                return games;
            }

            games = games.Where(g => !processedGameIds.Contains(g.GameId)).ToList();

            return games;
        }

        /// <summary>
        /// Inserts each of the <see cref="Game"/>s into the database.
        /// </summary>
        /// <param name="games"></param>
        /// <returns></returns>
        public async Task InsertGames(List<Game> games)
        {
            foreach (var game in games)
            {
                await chessRepository.InsertGame(game);
            }
        }
    }
}

