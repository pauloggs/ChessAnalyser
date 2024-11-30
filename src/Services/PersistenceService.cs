using Interfaces.DTO;
using Repositories;

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
		List<Game> GetUnprocessedGames(List<Game> games);

        Task InsertGames(List<Game> games);
	}

	public class PersistenceService(IChessRepository chessRepository) : IPersistenceService
    {
        private readonly Repositories.IChessRepository chessRepository = chessRepository;


        public List<Game> GetUnprocessedGames(List<Game> games)
        {
            // TODO. Implement a call to the ChessRepository to see if the game has already been processed.
            return games;
        }

        public async Task InsertGames(List<Game> games)
        {
            foreach (var game in games)
            {
                await chessRepository.InsertGame(game);
            }
        }
    }
}

