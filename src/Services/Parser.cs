using Interfaces.DTO;

namespace Services
{
    public interface IParser
    {
        /// <summary>
        /// Gets raw games from the provided PGN file.
        /// </summary>
        /// <param name="rawPgn"></param>
        /// <returns></returns>
        List<RawGame> GetRawGamesFromPgnFile(RawPgn rawPgn);

        /// <summary>
        /// Converts a raw game into an actual game, complete with
        /// tags and moves.
        /// </summary>
        /// <param name="rawGames"></param>
        /// <returns></returns>
        List<Game> GeGamesFromRawGames(List<RawGame> rawGames);

        /// <summary>
        /// Gets the game tags (Event, Location etc.) from the raw game contents.
        /// </summary>
        /// <param name="rawGameContent"></param>
        /// <returns></returns>
        Dictionary<string, string> GetGameTags(string rawGameContent);
    }

    public class Parser : IParser
    {
        public List<Game> GeGamesFromRawGames(List<RawGame> rawGames)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, string> GetGameTags(string rawGameContent)
        {
            throw new NotImplementedException();
        }

        public List<RawGame> GetRawGamesFromPgnFile(RawPgn rawPgn)
        {
            var returnValue = new List<RawGame>();

            var gameStartMarker = "[Event";            

            if (!rawPgn.Contents.Contains(gameStartMarker)) { return returnValue; }

            var pgnFileName = rawPgn.Name;

            string[] tokens = rawPgn.Contents.Split(new[] { gameStartMarker }, StringSplitOptions.None);
            
            foreach (string token in tokens)
            {
                if (token.Length > 0)
                {
                    returnValue.Add(new RawGame()
                    {
                        ParentPgnFileName = pgnFileName,
                        GameName = "SomeGameName",
                        Contents = (gameStartMarker + token).Trim()
                    });
                }
            }
            
            return returnValue;
        }
    }


}
