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
        List<Game> GetGamesFromPgnFiles(List<PgnFile> pgnGames);

        /// <summary>
        /// Extracts games from a single PGN file. Used to parse and persist one file at a time.
        /// </summary>
        List<Game> GetGamesFromPgnFile(PgnFile pgnFile);

        /// <summary>
        /// Enumerates games from a single PGN file one at a time to minimize memory (no full list in memory).
        /// </summary>
        IEnumerable<Game> EnumerateGamesFromPgnFile(PgnFile pgnFile);

        /// <summary>
        /// Returns the number of games in the PGN file (for progress reporting). Lightweight: only parses to split games.
        /// </summary>
        int GetGameCountInFile(PgnFile pgnFile);

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

        /// <summary>
        /// Extracts and parses a collection of chess games from a list of PGN files.
        /// </summary>
        /// <remarks>This method processes each PGN file to extract individual games, converts them into
        /// <see cref="Game"/> objects,  and assigns a unique identifier to each game based on its moves. The method
        /// assumes that the input PGN files  are valid and contain parsable chess game data.</remarks>
        /// <param name="pgnFiles">A list of <see cref="PgnFile"/> objects representing the PGN files to process.</param>
        /// <returns>A list of <see cref="Game"/> objects parsed from the provided PGN files. Each game includes a unique
        /// identifier.</returns>
        public List<Game> GetGamesFromPgnFiles(List<PgnFile> pgnFiles)
        {
            List<Game> games = [];
            foreach (var pgnFile in pgnFiles)
                games.AddRange(GetGamesFromPgnFile(pgnFile));
            return games;
        }

        /// <summary>
        /// Extracts and parses games from a single PGN file. Sets SourcePgnFileName and GameIndexInFile on each game.
        /// </summary>
        public List<Game> GetGamesFromPgnFile(PgnFile pgnFile)
        {
            var games = new List<Game>();
            var extractedPgnGames = PgnParserHelper.GetPgnGamesFromPgnFile(pgnFile);
            var gameIndex = 0;
            foreach (var pgnGame in extractedPgnGames)
            {
                gameIndex++;
                var game = PgnParserHelper.GetGameFromPgnGame(pgnGame);
                game.SourcePgnFileName = pgnFile.Name;
                game.GameIndexInFile = gameIndex;
                games.Add(game);
            }
            foreach (var game in games)
                game.GameId = GameIdGenerator.GetGameId(game.Plies);
            return games;
        }

        /// <summary>
        /// Yields one game at a time from the PGN file. Only one full Game is in memory at a time.
        /// </summary>
        public IEnumerable<Game> EnumerateGamesFromPgnFile(PgnFile pgnFile)
        {
            var extractedPgnGames = PgnParserHelper.GetPgnGamesFromPgnFile(pgnFile);
            var gameIndex = 0;
            foreach (var pgnGame in extractedPgnGames)
            {
                gameIndex++;
                var game = PgnParserHelper.GetGameFromPgnGame(pgnGame);
                game.SourcePgnFileName = pgnFile.Name;
                game.GameIndexInFile = gameIndex;
                game.GameId = GameIdGenerator.GetGameId(game.Plies);
                yield return game;
            }
        }

        public int GetGameCountInFile(PgnFile pgnFile)
        {
            return PgnParserHelper.GetPgnGamesFromPgnFile(pgnFile).Count;
        }

        public void SetBoardPositions(List<Game> games) => boardPositionGenerator.SetBoardPositions(games);
    }
}
