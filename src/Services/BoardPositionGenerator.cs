using Interfaces.DTO;

namespace Services
{
	public interface IBoardPositionGenerator
    {
        BoardPosition GetBoardPositionFromMove(BoardPosition previousBoardPosition, string rawMove);

        BoardPosition GetStartingBoardPosition();

        void SetBoardPositions(List<Game> games);
    }

    /// <summary>
    /// Main function is to convert PGN moves, such as d4, Nf6 etc.
    /// into BoardPosition objects, based on the previous BoardPosition
    /// object.
    /// </summary>
    public class BoardPositionGenerator : IBoardPositionGenerator
    {
        private readonly IMoveInterpreter _moveInterpreter;

		public BoardPositionGenerator(IMoveInterpreter moveInterpreter)
		{
            _moveInterpreter = moveInterpreter;
		}

        public BoardPosition GetBoardPositionFromMove(BoardPosition previousBoardPosition, string rawMove)
        {
            var boardPosition = new BoardPosition();

            var result = _moveInterpreter.GetDestinationRankAndFile(rawMove);

            return boardPosition;
        }

        public BoardPosition GetStartingBoardPosition()
        {
            var startingBoardPosition = new BoardPosition();

            // set white pieces
            startingBoardPosition.Pawns[0] = 8+9+10+11+12+13+14+15; // 2nd rank
            startingBoardPosition.Knights[0] = 1 + 6;
            startingBoardPosition.Bishops[0] = 2 + 5;
            startingBoardPosition.Rooks[0] = 0 + 7;
            startingBoardPosition.Queens[0] = 3;
            startingBoardPosition.Kings[1] = 4;

            // set black pieces
            startingBoardPosition.Pawns[1] = 48+49+50+51+52+53+54+55; // 7th rank
            startingBoardPosition.Kings[1] = 57 + 62;
            startingBoardPosition.Bishops[1] = 58 + 61;
            startingBoardPosition.Rooks[1] = 56 + 63;
            startingBoardPosition.Queens[1] = 59;
            startingBoardPosition.Kings[1] = 60;

            return startingBoardPosition;
        }

        public void SetBoardPositions(List<Game> games)
        {
            foreach (var game in games)
            {
                // set starting position
                // display board

                // loop through each ply
                //  from the previous position, apply the ply to get the current position
                //  display board
            }
        }
    }
}

