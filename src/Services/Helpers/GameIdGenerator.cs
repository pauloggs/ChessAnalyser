using Interfaces.DTO;

namespace Services.Helpers
{   
    public class GameIdGenerator
    {
        /// <summary>
        /// Generates the game id from the game's moves, then checks the database
        /// to see if it has already been processed. If not, the game id is returned.
        /// </summary>
        public static string CheckAndReturnGameId(Dictionary<int, Ply> plies)
        {
            return string.Join("|", plies.Select(x => x.Value.RawMove));
        }
    }
}

