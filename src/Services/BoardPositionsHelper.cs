using System;
using System.Text;
using Interfaces.DTO;

namespace Services
{
	public interface IBoardPositionsHelper
	{
		BoardPosition GetStartingBoardPosition();

        void DisplayBoardPosition(BoardPosition boardPosition);
    }

    public class BoardPositionsHelper : IBoardPositionsHelper
    {
        public BoardPosition GetStartingBoardPosition()
        {
            var startingBoardPosition = new BoardPosition();

            // set white pieces
            startingBoardPosition.Pawns[0] = 0b_1111_1111_0000_0000;

            // set black pieces                 8         7         6         5         4         3         2         1
            startingBoardPosition.Pawns[1] = 0b_0000_0000_1111_1111_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000; // 7th rank

            return startingBoardPosition;
        }

        public void DisplayBoardPosition(BoardPosition boardPosition)
        {
            var pawnBoard = new StringBuilder();

            for (var col = 0; col <=1; col++)
            {
                // display white pawns
                for (var rank = 8; rank >= 1; rank--)
                {
                    pawnBoard.Append($"{rank} |");

                    for (var file = 1; file <= 8; file++)
                    {
                        var bit = GetBit(boardPosition.Pawns[col], ((rank - 1) * 8) + (file - 1)) ? 1 : 0;
                        pawnBoard.Append(bit);
                    }

                    pawnBoard.AppendLine();
                }

                Console.Write(pawnBoard);
                Console.WriteLine();
                pawnBoard.Clear();
            }            
        }

        private static bool GetBit(ulong piecePositions, int index) => (piecePositions & (1ul << (index))) > 0;
    }
}

