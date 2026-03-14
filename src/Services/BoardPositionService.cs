using Interfaces;
using Interfaces.DTO;
using Services.Helpers;

namespace Services
{
    public interface IBoardPositionService
    {
        /// <summary>
        /// Set the board positions for all the games provided.
        /// Games that fail to parse (illegal/ambiguous move, invalid PGN) are removed from the list.
        /// </summary>
        /// <param name="games">Games to process; invalid games are removed in place.</param>
        /// <param name="parseErrors">Optional. When provided, each failed game is recorded here (source file, index, name, message) for persistence.</param>
        void SetBoardPositions(List<Game> games, List<GameParseError>? parseErrors = null);
    }

    /// <summary>
    /// Main function is to convert PGN moves, such as d4, Nf6 etc.
    /// into BoardPosition objects, based on the previous BoardPosition
    /// object.
    /// </summary>
    public class BoardPositionService : IBoardPositionService
    {
        private readonly IBoardPositionsHelper boardPositionsHelper;

        public BoardPositionService(
            IBoardPositionsHelper boardPositionsHelper)
		{
            this.boardPositionsHelper = boardPositionsHelper;
        }

        public void SetBoardPositions(List<Game> games, List<GameParseError>? parseErrors = null)
        {
            var invalidGames = new List<Game>();

            foreach (var game in games)
            {
                try
                {
                    Console.WriteLine($"Setting board positions for game '{game.Name}'");

                    // Set winner from PGN Result tag so it is correct even if result is not the last ply token
                    if (game.Tags != null &&
                        game.Tags.TryGetValue("result", out var resultValue) &&
                        Constants.GameEndConditions.TryGetValue(resultValue.Trim(), out var winner))
                    {
                        game.Winner = winner;
                    }

                    var startingBoardPosition = boardPositionsHelper.GetStartingBoardPosition();
                    game.InitialBoardPosition = startingBoardPosition;

                    game.BoardPositions[-1] = startingBoardPosition;

                    var numberOfPlies = game.Plies.Keys.Count;

                    for (var plyIndex = 0; plyIndex < numberOfPlies; plyIndex++)
                    {
                        //Console.WriteLine($"\nPly {plyIndex}, move {(plyIndex / 2) + 1}, {game.Plies[plyIndex].Colour}, {game.Plies[plyIndex].RawMove}");
                        if (boardPositionsHelper.SetWinner(game, plyIndex)) break;

                        var boardPositionFromPly = boardPositionsHelper.GetBoardPositionForPly(
                            game,
                            plyIndex);

                        game.BoardPositions[plyIndex] = boardPositionFromPly;

                        if (Constants.DisplayBoardPositions) PrintBoardPosition.Print(boardPositionFromPly);
                    }
                }
                catch (Exception ex)
                {
                    // Any parse failure (invalid move, illegal/ambiguous move): omit this game and optionally record for persistence.
                    invalidGames.Add(game);
                    parseErrors?.Add(new GameParseError
                    {
                        SourcePgnFileName = game.SourcePgnFileName,
                        GameIndexInFile = game.GameIndexInFile,
                        GameName = game.Name,
                        ErrorMessage = ex.Message
                    });
                }
            }

            foreach (var g in invalidGames)
                games.Remove(g);
        }
    }
}

