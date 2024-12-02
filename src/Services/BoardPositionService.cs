using Interfaces.DTO;

namespace Services
{
    public interface IBoardPositionService
    {    
        void SetBoardPositions(List<Game> games);

        void DisplayBoardPosition(BoardPosition boardPosition);
    }

    /// <summary>
    /// Main function is to convert PGN moves, such as d4, Nf6 etc.
    /// into BoardPosition objects, based on the previous BoardPosition
    /// object.
    /// </summary>
    public class BoardPositionService : IBoardPositionService
    {
        private readonly IMoveInterpreter _moveInterpreter;
        private readonly IBoardPositionsHelper _boardPositionsHelper;
        private readonly IDisplayService _displayService;

        public BoardPositionService(
            IMoveInterpreter moveInterpreter,
            IBoardPositionsHelper boardPositionsHelper,
            IDisplayService displayService)
		{
            _moveInterpreter = moveInterpreter;
            _boardPositionsHelper = boardPositionsHelper;
            _displayService = displayService;
        }

        public void SetBoardPositions(List<Game> games)
        {
            foreach (var game in games)
            {
                game.BoardPositions[0] = _boardPositionsHelper.GetStartingBoardPosition();

                // TODO delete this test display
                var boardArray = _displayService.GetBoardArrayFromBoardPositions(game.BoardPositions[0]);
                _displayService.DisplayBoardPosition(boardArray);
            }
        }

        public void DisplayBoardPosition(BoardPosition boardPosition)
        {
            throw new NotImplementedException();
        }
    }
}

