using Interfaces;
using Interfaces.DTO;
using System.Text.RegularExpressions;

namespace Services.Helpers
{
    public static class PgnParserHelper
    {
        /// <summary>
        /// Extracts a list of separate PGN games from the contents of a PGN (Portable Game Notation) file.
        /// </summary>
        /// <remarks>This method identifies individual games in the PGN file by splitting the file's
        /// contents using a predefined game start marker. Each extracted game includes the original game start marker
        /// and is associated with the name of the PGN file it was extracted from.</remarks>
        /// <param name="pgnFile">The PGN file represented as a <see cref="PgnFile"/> object, containing the file name and its contents.</param>
        /// <returns>A list of <see cref="PgnGame"/> objects, where each object represents a single raw chess game extracted from
        /// the PGN file.  If no games are found, an empty list is returned.</returns>
        public static List<PgnGame> GetPgnGamesFromPgnFile(PgnFile pgnFile)
        {
            var rawGames = new List<PgnGame>();

            if (!pgnFile.Contents.Contains(Constants.GameStartMarker)) { return rawGames; }

            var pgnFileName = pgnFile.Name;

            string[] tokens = pgnFile.Contents.Split([Constants.GameStartMarker], StringSplitOptions.None);

            // Exit if there are no 'start of game' markers
            if (tokens.Length == 0) { return rawGames; }

            // Process each token
            foreach (string token in tokens)
            {
                // If the token has any contents, add to the raw games
                if (token.Length > 0)
                {
                    // Re-add the GameStartMarker
                    var gameContents = (Constants.GameStartMarker + token).Trim();

                    rawGames.Add(new PgnGame()
                    {
                        ParentPgnFileName = pgnFileName,
                        Contents = gameContents
                    });
                }
            }

            return rawGames;
        }

        /// <summary>
        /// Converts a PGN (Portable Game Notation) game into a <see cref="Game"/> object.
        /// </summary>
        /// <remarks>This method processes the contents of the provided <see cref="PgnGame"/> to extract
        /// metadata tags and move information. The resulting <see cref="Game"/> object includes a dictionary of tags
        /// and a dictionary of plies, which represent the moves in the game.</remarks>
        /// <param name="pgnGame">The PGN game to convert, represented as a <see cref="PgnGame"/> object.</param>
        /// <returns>A <see cref="Game"/> object containing the parsed tags and plies from the PGN game.</returns>
        public static Game GetGameFromPgnGame(PgnGame pgnGame)
        {
            var pgnGameContent = pgnGame.Contents;
            var pgnGameLines = Regex.Split(pgnGameContent, "\r\n|\r|\n");
            var plyNumber = 0;
            var tagDictionary = new Dictionary<string, string>();
            var plyDictionary = new Dictionary<int, Ply>();

            // Loop through each line in the raw game
            foreach (var pgnGameLine in pgnGameLines)
            {
                string line = pgnGameLine.Trim();
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line.StartsWith("["))
                {
                    TagHelper.AddGameTag(tagDictionary, line);
                }
                else
                {
                    PlyHelper.AddPlies(plyDictionary, line, ref plyNumber);
                }
            }
            var gameName = GameNameHelper.GetGameName(tagDictionary);
            var game = new Game()
            {
                Name = gameName,
                Tags = tagDictionary,
                Plies = plyDictionary
            };

            return game;
        }      
    }
}
