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
        Task InsertGames(List<Game> games);

        /// <summary>
        /// Persists parse errors for diagnostics. A failure here does not throw; errors are logged so that
        /// persistence of valid games is never interrupted.
        /// </summary>
        Task InsertParseErrors(List<GameParseError> parseErrors);
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
                var gameId = await chessRepository.InsertGame(game);
                await chessRepository.InsertBoardPositions(game, gameId);
            }
        }

        /// <summary>
        /// Inserts parse error records. Catches and logs any failure so that persistence of valid games is never interrupted.
        /// </summary>
        public async Task InsertParseErrors(List<GameParseError> parseErrors)
        {
            if (parseErrors == null || parseErrors.Count == 0)
                return;
            try
            {
                foreach (var error in parseErrors)
                    await chessRepository.InsertGameParseError(error);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to persist parse errors ({parseErrors.Count} record(s)): {ex.Message}");
            }
        }
    }
}

