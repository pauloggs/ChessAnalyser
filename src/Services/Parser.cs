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

    public class Parser(INaming naming) : IParser
    {
        private readonly INaming naming = naming;

        private readonly List<string> GameTagIdentifiers = new List<string>()
        {
            "Event",
            "Site",
            "Date",
            "Round",
            "White",
            "Black",
            "Result",
            "WhiteElo",
            "BlackElo",
            "ECO"
        };

        public List<Game> GeGamesFromRawGames(List<RawGame> rawGames)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, string> GetGameTags(string rawGameContent)
        {
            var gameTags = new Dictionary<string, string>();

            foreach (var tag in GameTagIdentifiers)
            {
                gameTags[tag] = GetTag(tag, rawGameContent);
            }

            return gameTags;
        }

        public List<RawGame> GetRawGamesFromPgnFile(RawPgn rawPgn)
        {
            var rawGames = new List<RawGame>();

            var gameStartMarker = "[Event";            

            if (!rawPgn.Contents.Contains(gameStartMarker)) { return rawGames; }

            var pgnFileName = rawPgn.Name;

            string[] tokens = rawPgn.Contents.Split(new[] { gameStartMarker }, StringSplitOptions.None);
            
            foreach (string token in tokens)
            {
                if (token.Length > 0)
                {                  
                    var gameContents = (gameStartMarker + token).Trim();

                    var gameTags = GetGameTags(gameContents);

                    var gameName = naming.GetGameName(gameTags);

                    rawGames.Add(new RawGame()
                    {
                        ParentPgnFileName = pgnFileName,
                        GameName = gameName,
                        Contents = gameContents
                    });
                }
            }
            
            return rawGames;
        }

        private static string GetTag(string tag, string gameContents)
        {
            var startLocationOfTag = gameContents.IndexOf(tag);
            if (startLocationOfTag == -1) { return tag; }

            var contentsStartingWithTag = gameContents.Substring(startLocationOfTag + tag.Length);

            var endOfTagLocation = contentsStartingWithTag.IndexOf("]");

            var tagValue = contentsStartingWithTag.Substring(0, endOfTagLocation).Replace("\"", "").Trim();

            return tagValue;
        }
    }
}
