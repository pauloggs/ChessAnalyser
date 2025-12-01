using Interfaces;
using Interfaces.DTO;
using Services.Helpers;
using System.Security.Cryptography;
using System.Text;
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
                var game = PgnParserHelper.GetGameFromPgnGame(pgnGame);
                games.Add(game);
            }

            // Generate GameId for each game
            foreach (var game in games)
            {
                game.GameId = GameIdGenerator.GetGameId(game.Plies);
            }

            return games;
        }

        public void SetBoardPositions(List<Game> games) => boardPositionGenerator.SetBoardPositions(games);
    }
}
