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
        private readonly IDisplayService displayService;

        public BoardPositionService(
            IBoardPositionsHelper boardPositionsHelper,
            IDisplayService displayService)
		{
            this.boardPositionsHelper = boardPositionsHelper;
            this.displayService = displayService;
        }

        public void SetBoardPositions(List<Game> games)
        {
            foreach (var game in games)
            {
                game.BoardPositions[0] = boardPositionsHelper.GetStartingBoardPosition();

                // moved here from the helper
                var numberOfPlies = game.Plies.Keys.Count;

                // loop through all the plies of this game, and set the board positions for each ply
                for (var plyIndex = 0; plyIndex < numberOfPlies; plyIndex++)
                {
                    // check for game result, 1-0, 0-1, 1/2-1/2, and set the winner (White,Black or None) if found
                    if (boardPositionsHelper.SetWinner(game, plyIndex)) break;

                    // ply 0 is applied to create the first board position, so current board index is ply index + 1
                    var currentBoardIndex = plyIndex + 1;

                    // get the previous board position, which is needed to calculate the current one
                    var previousBoardPosition = game.BoardPositions[plyIndex];

                    // set the current board position from the previous one and the current ply
                    boardPositionsHelper.SetBoardPositionFromPly(
                        game, 
                        previousBoardPosition, 
                        game.Plies[plyIndex], 
                        currentBoardIndex);
                }
            }
        }
    }
}

