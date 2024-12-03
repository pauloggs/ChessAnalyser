using Interfaces.DTO;

namespace Services
{
    public interface IBoardPositionService
    {    
        void SetBoardPositions(List<Game> games);
    }

    /// <summary>
    /// Main function is to convert PGN moves, such as d4, Nf6 etc.
    /// into BoardPosition objects, based on the previous BoardPosition
    /// object.
    /// </summary>
    public class BoardPositionService : IBoardPositionService
    {
        private readonly IBoardPositionsHelper _boardPositionsHelper;
        private readonly IDisplayService _displayService;

        public BoardPositionService(
            IBoardPositionsHelper boardPositionsHelper,
            IDisplayService displayService)
		{
            _boardPositionsHelper = boardPositionsHelper;
            _displayService = displayService;
        }

        public void SetBoardPositions(List<Game> games)
        {
            foreach (var game in games)
            {
                game.BoardPositions[0] = _boardPositionsHelper.GetStartingBoardPosition();

                // loop through all plies and set board positions

                // TODO delete this test display
                var boardArray = _displayService.GetBoardArrayFromBoardPositions(game.BoardPositions[0]);
                _displayService.DisplayBoardPosition(boardArray);

                _boardPositionsHelper.SetBoardPositions(game);
            }
        }
    }
}

