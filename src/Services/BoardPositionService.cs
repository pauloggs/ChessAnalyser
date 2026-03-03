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
            IBoardPositionsHelper boardPositionsHelper)
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

                var numberOfPlies = game.Plies.Keys.Count;

                for (var plyIndex = 0; plyIndex < numberOfPlies; plyIndex++)
                {
                    Console.WriteLine($"\nPly {plyIndex}, move {(plyIndex / 2) + 1}, {game.Plies[plyIndex].Colour}, {game.Plies[plyIndex].RawMove}");
                    if (boardPositionsHelper.SetWinner(game, plyIndex)) break;

                    var boardPositionFromPly = boardPositionsHelper.GetBoardPositionForPly(
                        game,
                        plyIndex);

                    game.BoardPositions[plyIndex] = boardPositionFromPly;

                    PrintBoardPosition.Print(boardPositionFromPly);
                }
            }
        }
    }
}

