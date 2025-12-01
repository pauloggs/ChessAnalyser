using Interfaces;
using Interfaces.DTO;
using Services.Helpers;
using System.Text.RegularExpressions;

namespace Services
{
    public interface IPgnParser
    {
        /// <summary>
        /// Extracts a collection of chess games from a list of PGN (Portable Game Notation) files.
        /// </summary>
        /// <param name="pgnGames">A list of <see cref="PgnFile"/> objects representing the raw PGN files to process.</param>
        /// <returns>A list of <see cref="Game"/> objects representing the chess games parsed from the provided PGN files.  The
        /// list will be empty if no games are found.</returns>
        List<Game> GetGamesFromPgnFiles(List<PgnFile> pgnGames);

        /// <summary>
        /// Updates the board positions based on the provided list of games.
        /// </summary>
        /// <remarks>Each game in the list is used to determine the new state of the board positions.
        /// Ensure that the list contains valid game objects.</remarks>
        /// <param name="games">A list of <see cref="Game"/> objects representing the games to use for updating the board positions. Cannot
        /// be null.</param>
        void SetBoardPositions(List<Game> games);
    }

    public class PgnParser(
        INaming naming,
        IBoardPositionService boardPositionGenerator) : IPgnParser
    {
        private readonly INaming naming = naming;  

        public List<Game> GetGamesFromPgnFiles(List<PgnFile> pgnFiles)
        {
            List<PgnGame> pgnGames = [];
            List<Game> games = [];

            // Get the PgnGame objects from the PGN files
            foreach (var pgnFile in pgnFiles)
            {
                var extractedPgnGames = PgnParserHelper.GetPgnGamesFromPgnFile(pgnFile);
                pgnGames.AddRange(extractedPgnGames);
            }

            // Parse each PgnGame into a Game object
            foreach (var pgnGame in pgnGames)
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
                        AddGameTage(tagDictionary, line);
                    }
                    else
                    {
                        AddPlies(plyDictionary, line, ref plyNumber);
                    }
                }
                var gameId = GameIdGenerator.CheckAndReturnGameId(plyDictionary);
                var gameName = GetGameName(tagDictionary);
                var game = new Game() 
                {
                    Name = gameName,
                    Tags = tagDictionary,
                    Plies = plyDictionary,
                    GameId = gameId
                };
                games.Add(game);
            }

            //// Loop through each PGN file and extract games
            //foreach (var rawPgn in pgnFiles)
            //{
            //    // Extract PGN games from the PGN file
            //    var pgnGames = PgnParserHelper.GetPgnGamesFromPgnFile(rawPgn);

            //    // Loop through each PGN game in the PGN file
            //    foreach (var pgnGame in pgnGames)
            //    {
            //        var pgnGameContent = pgnGame.Contents;

            //        var pgnGameLines = Regex.Split(pgnGameContent, "\r\n|\r|\n");

            //        var plyNumber = 0;

            //        var tagDictionary = new Dictionary<string, string>();

            //        var plyDictionary = new Dictionary<int, Ply>();

            //        // Loop through each line in the raw game
            //        foreach (var pgnGameLine in pgnGameLines)
            //        {
            //            string line = pgnGameLine.Trim();

            //            if (string.IsNullOrWhiteSpace(line)) continue;

            //            if (line.StartsWith("["))
            //            {
            //                AddGameTage(tagDictionary, line);
            //            }
            //            else
            //            {
            //                AddPlies(plyDictionary, line, ref plyNumber);
            //            }
            //        }

            //        var gameId = gameIdGenerator.CheckAndReturnGameId(plyDictionary);

            //        var gameName = GetGameName(tagDictionary);

            //        var game = new Game() 
            //        {
            //            Name = gameName,
            //            Tags = tagDictionary,
            //            Plies = plyDictionary,
            //            GameId = gameId
            //        };

            //        games.Add(game);
            //    }
            //}

            return games;
        }

        private static void AddGameTage(Dictionary<string, string> tagDictionary, string line)
        {
            var tagSections = line.Split(" ", 2);

            string tagKey = tagSections[0].Trim('[').ToLower();

            string tagValue = tagSections[1].Trim(']').Replace("\"","");

            tagDictionary[tagKey] = tagValue;
        }

        private static void AddPlies(Dictionary<int,Ply> plyDictionary, string line, ref int plyNumber)
        {
            Regex moveNumbersRegex = new Regex(@"\d+\.");

            line = moveNumbersRegex.Replace(line, "");

            var plies = line.Split(" ");

            foreach (string plyString in plies)
            {
                if (!string.IsNullOrWhiteSpace(plyString))
                {
                    var ply = new Ply()
                    {
                        MoveNumber = (plyNumber / 2) + 1,
                        RawMove = plyString,
                        Colour = plyNumber % 2 == 0 ? 'W' : 'B'
                    };

                    plyDictionary[plyNumber] = ply;

                    plyNumber++;
                }                
            }
        }

        private static string GetGameName(Dictionary<string, string> tagDicionary)
        {
            var tagList = new List<string>();

            foreach (var tag in Constants.GameTagIdentifiers)
            {
                if (tagDicionary.ContainsKey(tag))
                {
                    tagList.Add(tagDicionary[tag]);
                }
                else
                {
                    tagList.Add(Constants.DefaultEmptyTagValue);
                }
            }

            return string.Join("|", tagList);
        }

        public void SetBoardPositions(List<Game> games) => boardPositionGenerator.SetBoardPositions(games);
    }
}
