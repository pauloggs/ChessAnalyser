using Interfaces;
using Interfaces.DTO;
using Services.Helpers;

namespace Services
{
    public interface IBoardPositionService
    {
        /// <summary>
        /// Set the board positions for all the games provided.
        /// </summary>
        /// <param name="games"></param>
        void SetBoardPositions(List<Game> games);
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
            IBoardPositionsHelper boardPositionsHelper,
            IDisplayService displayService)
		{
            this.boardPositionsHelper = boardPositionsHelper;
        }

        public void SetBoardPositions(List<Game> games)
        {
            foreach (var game in games)
            {
                Console.WriteLine($"Setting board positions for game '{game.Name}'");

                // initialize the starting position
                var startingBoardPosition = boardPositionsHelper.GetStartingBoardPosition();
                game.InitialBoardPosition = startingBoardPosition;

                // Maintain legacy index -1 for compatibility with any existing code/tests.
                game.BoardPositions[-1] = startingBoardPosition;

                // moved here from the helper
                var numberOfPlies = game.Plies.Keys.Count;

                // loop through each ply to determine the board position
                for (var plyIndex = 0; plyIndex < numberOfPlies; plyIndex++)
                {
                    Console.WriteLine($"\nPly {plyIndex}, move {(plyIndex / 2) + 1}, {game.Plies[plyIndex].Colour}, {game.Plies[plyIndex].RawMove}");
                    // check for game result, 1-0, 0-1, 1/2-1/2, and set the winner (White,Black or None) if found
                    // if winner is set, break the loop as no more board positions are needed
                    if (boardPositionsHelper.SetWinner(game, plyIndex)) break;

                    var boardPositionFromPly = boardPositionsHelper.GetBoardPositionForPly(
                        game,
                        plyIndex);

                    game.BoardPositions[plyIndex] = boardPositionFromPly;

                    // display the board position in the console for debugging
                    PrintBoardPosition.Print(boardPositionFromPly);
                }
            }
        }
    }
}

