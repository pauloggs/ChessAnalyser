using Interfaces.DTO;

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
	}

	public class PersistenceService : IPersistenceService
    {
		public PersistenceService()
		{
		}

        public List<Game> GetUnprocessedGames(List<Game> games)
        {
            // TODO. Implement a call to the ChessRepository to see if the game has already been processed.
            return games;
        }
    }
}

