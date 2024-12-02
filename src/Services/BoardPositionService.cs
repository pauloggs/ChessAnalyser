using System.Text;
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
        private readonly IMoveInterpreter _moveInterpreter;
        private readonly IBoardPositionsHelper _boardPositionsHelper;

        public BoardPositionService(IMoveInterpreter moveInterpreter, IBoardPositionsHelper boardPositionsHelper)
		{
            _moveInterpreter = moveInterpreter;
            _boardPositionsHelper = boardPositionsHelper;
		}

        public void SetBoardPositions(List<Game> games)
        {
            foreach (var game in games)
            {
                game.BoardPositions[0] = _boardPositionsHelper.GetStartingBoardPosition();

                // TODO. Remove test display
                _boardPositionsHelper.DisplayBoardPosition(game.BoardPositions[0]);
            }
        }

    }
}

