using System.Text;
using Interfaces;
using Interfaces.DTO;

namespace Services
{
	public interface IBoardPositionsHelper
	{
		BoardPosition GetStartingBoardPosition();

        void RemovePieceFromBoardPosition(BoardPosition boardPosition, char piece, int col, char file, int rank);

        void AddPieceFromBoardPosition(BoardPosition boardPosition, char piece, int col, char file, int rank);
    }

    public class BoardPositionsHelper : IBoardPositionsHelper
    {
        public BoardPosition GetStartingBoardPosition()
        {
            var startingBoardPosition = new BoardPosition();

            // set white pieces
            startingBoardPosition.PiecePositions["WP"] = 0b_1111_1101_0000_0000;

            // TODO. Remove test removal
            RemovePieceFromBoardPosition(startingBoardPosition, 'P', 0, 'b', 1);

            // set black pieces                 8         7         6         5         4         3         2         1
            startingBoardPosition.PiecePositions["BP"] = 0b_0000_0000_1111_1111_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000; // 7th rank

            return startingBoardPosition;
        }        


        public void RemovePieceFromBoardPosition(BoardPosition boardPosition, char piece, int col, char file, int rank)
        {
            var square = (ulong)Math.Pow(2, rank * 8 + Constants.File[file]);

            var colour = col == 0 ? "W" : "P";

            boardPosition.PiecePositions[colour + piece] &= ~square;
        }

        public void AddPieceFromBoardPosition(BoardPosition boardPosition, char piece, int col, char file, int rank)
        {
            var square = (ulong)Math.Pow(2, rank * 8 + Constants.File[file]);

            var colour = col == 0 ? "W" : "P";

            boardPosition.PiecePositions[colour + piece] |= square;
        }
    }
}

