using Interfaces.DTO;

namespace Services
{
    /// <summary>
    /// Generates the game id from the game's moves, then checks the database
    /// to see if it has already been processed. If not, the game id is returned.
    /// </summary>
	public interface IGameIdGenerator
	{
		string CheckAndReturnGameId(Dictionary<int, Ply> plies);
	}

    public class GameIdGenerator : IGameIdGenerator
    {
        public string CheckAndReturnGameId(Dictionary<int, Ply> plies)
        {
            return string.Join("|", plies.Select(x => x.Value.RawMove));
        }
    }
}

